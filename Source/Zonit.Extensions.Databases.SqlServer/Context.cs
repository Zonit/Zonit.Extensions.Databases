using Microsoft.EntityFrameworkCore;

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

public class DbContextFactoryAdapter<TContext>(IDbContextFactory<TContext> innerFactory) : IDbContextFactory<DbContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _innerFactory = innerFactory;

    public DbContext CreateDbContext() => _innerFactory.CreateDbContext();
}
