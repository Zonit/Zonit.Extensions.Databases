using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Zonit.Extensions.Databases.SqlServer;

/// <summary>
/// SQL Server repository with a specific DbContext type.
/// Use this when you need to derive from SqlServerRepository with a concrete DbContext.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
/// <typeparam name="TContext">The specific DbContext type.</typeparam>
/// <remarks>
/// <para>
/// Register your repository using <c>AddDbRepository</c> to enable automatic extension support:
/// </para>
/// <code>
/// services.AddDbRepository&lt;IBlogRepository, BlogRepository&gt;();
/// </code>
/// <para>
/// Your repository can have any dependencies in constructor - DI will inject them automatically.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple repository - no IServiceProvider needed!
/// internal class BlogRepository(IDbContextFactory&lt;DatabaseContext&gt; contextFactory)
///     : SqlServerRepository&lt;Blog, DatabaseContext&gt;(contextFactory), IBlogRepository;
/// 
/// // Repository with custom dependencies
/// internal class BlogRepository(
///     IDbContextFactory&lt;DatabaseContext&gt; contextFactory,
///     IMyCustomService myService)
///     : SqlServerRepository&lt;Blog, DatabaseContext&gt;(contextFactory), IBlogRepository
/// {
///     // Use myService here
/// }
/// </code>
/// </example>
[RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
[RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
public class SqlServerRepository<TEntity, TContext> : SqlServerRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _typedContextFactory;

    /// <summary>
    /// Gets the typed DbContext factory for creating database contexts.
    /// Use this when you need direct access to the specific DbContext type.
    /// </summary>
    protected new IDbContextFactory<TContext> ContextFactory => _typedContextFactory;

    /// <summary>
    /// Creates a new database context asynchronously.
    /// This is a convenience method for derived repositories that need direct database access.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new TContext instance. Remember to dispose it (use await using).</returns>
    protected new ValueTask<TContext> CreateContextAsync(CancellationToken cancellationToken = default)
        => new(_typedContextFactory.CreateDbContextAsync(cancellationToken));

    /// <summary>
    /// Creates a new SQL Server repository for a specific DbContext type.
    /// </summary>
    /// <param name="contextFactory">The EF Core DbContext factory for the specific context type.</param>
    /// <remarks>
    /// This constructor does not provide extension support (e.g., User loading).
    /// Use the constructor with IServiceProvider for full extension support.
    /// </remarks>
    public SqlServerRepository(IDbContextFactory<TContext> contextFactory)
        : base(new DbContextFactoryAdapter<TContext>(contextFactory), null, false)
    {
        _typedContextFactory = contextFactory;
    }

    /// <summary>
    /// Creates a new SQL Server repository for a specific DbContext type with extension support.
    /// </summary>
    /// <param name="contextFactory">The EF Core DbContext factory for the specific context type.</param>
    /// <param name="serviceProvider">Service provider for resolving extensions.</param>
    /// <remarks>
    /// Use this constructor to enable extension support (e.g., User loading via IDatabaseExtension).
    /// </remarks>
    public SqlServerRepository(IDbContextFactory<TContext> contextFactory, IServiceProvider serviceProvider)
        : base(new DbContextFactoryAdapter<TContext>(contextFactory), serviceProvider, false)
    {
        _typedContextFactory = contextFactory;
    }

    /// <summary>
    /// Creates a new SQL Server repository using RepositoryContext for simplified dependency injection.
    /// </summary>
    /// <param name="context">Repository context containing all required dependencies.</param>
    /// <remarks>
    /// <para>
    /// <b>Recommended constructor</b> - simplifies DI and provides full extension support.
    /// </para>
    /// <para>
    /// Register <see cref="RepositoryContext{TContext}"/> by calling <c>services.AddDbSqlServer&lt;TContext&gt;()</c>
    /// which automatically registers it as a scoped service.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// internal class BlogRepository(RepositoryContext&lt;DatabaseContext&gt; context)
    ///     : SqlServerRepository&lt;Blog, DatabaseContext&gt;(context), IBlogRepository;
    /// </code>
    /// </example>
    public SqlServerRepository(RepositoryContext<TContext> context)
        : base(new DbContextFactoryAdapter<TContext>(context.ContextFactory), context.ServiceProvider, false)
    {
        _typedContextFactory = context.ContextFactory;
    }
}
