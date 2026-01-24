using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Zonit.Extensions.Databases.Tests.Infrastructure;

/// <summary>
/// Helper for creating test database contexts with InMemory provider.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new IDbContextFactory for testing with InMemory database.
    /// </summary>
    public static IDbContextFactory<TestDbContext> Create(string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContextFactory<TestDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>();
    }

    /// <summary>
    /// Creates a service provider with IDbContextFactory for testing.
    /// </summary>
    public static IServiceProvider CreateServiceProvider(string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContextFactory<TestDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        return services.BuildServiceProvider();
    }
}
