using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zonit.Extensions.Databases.Abstractions.Options;
using Zonit.Extensions.Databases.Abstractions.Exceptions;
using Zonit.Extensions.Databases.SqlServer.Backgrounds;

namespace Zonit.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
    {
#if !DEBUG
        services.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });
#endif

        services.AddDbOptionsSqlServer();
        services.AddDbContextSqlServer<TContext>();
        services.AddDbMigrationSqlServer<TContext>();

        return services;
    }

    /// <summary>
    /// Inicjalize Database, config etc.
    /// Only register Program.cs
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
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

    public static IServiceCollection AddDbContextSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlServer($"Server={databaseOptions.Server};Database={databaseOptions.Name};user={databaseOptions.User};Password={databaseOptions.Password};{databaseOptions.Parameters}");
        });

        return services;
    }

    public static IServiceCollection AddDbMigrationSqlServer<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddHostedService<MigrationEvent<TContext>>();
        return services;
    }
}
