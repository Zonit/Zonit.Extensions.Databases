using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using Zonit.Extensions.Databases.SqlServer.Backgrounds;
using Zonit.Extensions.Databases.SqlServer;
using Zonit.Extensions.Databases;

#if !DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace Zonit.Extensions;

/// <summary>
/// Configuration options for database migrations.
/// </summary>
public sealed class MigrationOptions
{
    /// <summary>
    /// Whether to run migrations on application startup. Default: true.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Whether to use BackgroundService (non-blocking) for migrations. Default: true.
    /// When false, migrations run synchronously during startup (blocks app start).
    /// </summary>
    public bool UseBackgroundMigration { get; set; } = true;
}

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server database support with default options.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    [RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
    [RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
    public static IServiceCollection AddDbSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
        => services.AddDbSqlServer<TContext>(options => { });

    /// <summary>
    /// Adds SQL Server database support with configurable migration options.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureMigration">Optional migration configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    [RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
    [RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
    public static IServiceCollection AddDbSqlServer<TContext>(
        this IServiceCollection services,
        Action<MigrationOptions> configureMigration) where TContext : DbContext
    {
        var migrationOptions = new MigrationOptions();
        configureMigration(migrationOptions);

#if !DEBUG
        services.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });
#endif

        services.AddDbOptionsSqlServer();
        services.AddDbContextSqlServer<TContext>();

        if (migrationOptions.AutoMigrate)
        {
            services.AddDbMigrationSqlServer<TContext>();
        }

        services.AddScoped<Context<TContext>>();
        services.AddScoped<RepositoryContext<TContext>>();

        return services;
    }

    /// <summary>
    /// Inicjalize Database, config etc.
    /// Only register Program.cs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// Uses configuration binding which requires dynamic code generation for AOT.
    /// Consider using manual configuration for full AOT compatibility.
    /// </remarks>
    [RequiresUnreferencedCode("Configuration binding uses reflection.")]
    [RequiresDynamicCode("Configuration binding requires dynamic code generation.")]
    public static IServiceCollection AddDbOptionsSqlServer(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();

        var databaseSection = configuration?.GetSection("Database");
        if (databaseSection is null || databaseSection.Exists() is false)
            throw new DatabaseException("Database configuration section not found.");

        services.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>(
                (options, configuration) =>
                    configuration.GetSection("Database").Bind(options));

        return services;
    }

    [RequiresUnreferencedCode("EF Core DbContext registration uses reflection.")]
    [RequiresDynamicCode("EF Core DbContext requires dynamic code generation.")]
    public static IServiceCollection AddDbContextSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlServer($"Server={databaseOptions.Server};Database={databaseOptions.Name};user={databaseOptions.User};Password={databaseOptions.Password};{databaseOptions.Parameters}");
        });

        return services;
    }

    /// <summary>
    /// Adds database migration as a BackgroundService.
    /// Uses lazy context creation and caching to avoid redundant migration checks.
    /// </summary>
    /// <remarks>
    /// EF Core migrations are not supported with NativeAOT. Use migration bundles for AOT scenarios.
    /// </remarks>
    [RequiresDynamicCode("EF Core migrations require dynamic code generation.")]
    public static IServiceCollection AddDbMigrationSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddHostedService<MigrationEvent<TContext>>();
        return services;
    }

    /// <summary>
    /// Registers an IDatabaseExtension for AOT-safe resolution.
    /// Use this to register extensions that will be resolved via keyed services.
    /// </summary>
    /// <typeparam name="TExtension">The extension data type.</typeparam>
    /// <typeparam name="TImplementation">The extension implementation type.</typeparam>
    [RequiresDynamicCode("DI registration requires dynamic code for generic types.")]
    public static IServiceCollection AddDatabaseExtension<TExtension, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TExtension : class
        where TImplementation : class, IDatabaseExtension<TExtension>
    {
        var extensionKey = $"Extension:{typeof(TExtension).FullName}";

        // Register as keyed service for AOT-safe resolution
        services.AddKeyedScoped<IDatabaseExtension, TImplementation>(extensionKey);

        // Also register as generic interface for backwards compatibility
        services.TryAddScoped<IDatabaseExtension<TExtension>, TImplementation>();

        // Register non-generic for enumeration
        services.AddScoped<IDatabaseExtension>(sp => sp.GetRequiredService<IDatabaseExtension<TExtension>>());

        return services;
    }
}
