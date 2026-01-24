using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Zonit.Extensions.Databases.SqlServer;

public interface IContextBase
{
    DbContext LocalDbContext { get; }
    IDbContextFactory<DbContext> LocalDbContextFactory { get; }
    IServiceProvider ServiceProvider { get; }
}

public interface IContext<TContext> : IContextBase where TContext : DbContext
{
    new TContext LocalDbContext { get; }
    new IDbContextFactory<TContext> LocalDbContextFactory { get; }
    TContext DbContext { get; }
    IDbContextFactory<TContext> DbContextFactory { get; }
}

public abstract class ContextBase : IContextBase
{
    public required DbContext LocalDbContext { get; set; }
    public required IDbContextFactory<DbContext> LocalDbContextFactory { get; set; }
    public required IServiceProvider ServiceProvider { get; set; }
}

/// <summary>
/// Context wrapper that provides access to DbContext and factory.
/// </summary>
/// <remarks>
/// Uses Entity Framework Core which requires dynamic code generation.
/// </remarks>
[RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
[RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
public class Context<TContext> : ContextBase, IContext<TContext>
    where TContext : DbContext
{
    public TContext DbContext { get; }
    public IDbContextFactory<TContext> DbContextFactory { get; }

    public Context(TContext context, IDbContextFactory<TContext> contextFactory, IServiceProvider serviceProvider)
    {
        DbContext = context;
        DbContextFactory = contextFactory;

        LocalDbContext = context;
        ServiceProvider = serviceProvider;
        LocalDbContextFactory = new DbContextFactoryAdapter<TContext>(contextFactory);
    }

    // Implementacja interfejsu IContext<TContext> wykorzystując istniejące właściwości
    TContext IContext<TContext>.LocalDbContext => DbContext;
    IDbContextFactory<TContext> IContext<TContext>.LocalDbContextFactory => DbContextFactory;
}

/// <summary>
/// Adapter to convert typed IDbContextFactory to non-generic one.
/// </summary>
/// <remarks>
/// Uses Entity Framework Core which requires dynamic code generation.
/// </remarks>
[RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
[RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
public class DbContextFactoryAdapter<TContext>(IDbContextFactory<TContext> innerFactory) : IDbContextFactory<DbContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _innerFactory = innerFactory;

    public DbContext CreateDbContext() => _innerFactory.CreateDbContext();
}
