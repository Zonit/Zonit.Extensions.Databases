using Microsoft.EntityFrameworkCore;

namespace Zonit.Extensions.Databases.SqlServer;

/// <summary>
/// Provides a unified context for repository operations, containing
/// the database context factory and service provider.
/// </summary>
/// <typeparam name="TContext">The type of the database context.</typeparam>
/// <param name="contextFactory">The factory for creating database context instances.</param>
/// <param name="serviceProvider">The service provider for resolving dependencies.</param>
public sealed class RepositoryContext<TContext>(
    IDbContextFactory<TContext> contextFactory,
    IServiceProvider serviceProvider)
    where TContext : DbContext
{
    /// <summary>
    /// Gets the factory for creating database context instances.
    /// </summary>
    public IDbContextFactory<TContext> ContextFactory => contextFactory;

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider => serviceProvider;

    /// <summary>
    /// Creates a new database context instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new database context instance.</returns>
    public Task<TContext> CreateContextAsync(CancellationToken cancellationToken = default)
        => contextFactory.CreateDbContextAsync(cancellationToken);

    /// <summary>
    /// Creates a new database context instance.
    /// </summary>
    /// <returns>A new database context instance.</returns>
    public TContext CreateContext()
        => contextFactory.CreateDbContext();
}
