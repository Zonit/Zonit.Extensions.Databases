using Microsoft.EntityFrameworkCore;

namespace Zonit.Extensions.Databases.SqlServer;

public abstract class ContextBase
{
    public required DbContext DbContext { get; set; }
    public required IDbContextFactory<DbContext> DbContextFactory { get; set; }
    public required IServiceProvider ServiceProvider { get; set; }
}

public class Context<TContext> : ContextBase
    where TContext : DbContext
{
    public Context(TContext context, IDbContextFactory<TContext> contextFactory, IServiceProvider serviceProvider)
    {
        DbContext = context;
        ServiceProvider = serviceProvider;
        DbContextFactory = new DbContextFactoryAdapter<TContext>(contextFactory);
    }
}

public class DbContextFactoryAdapter<TContext>(IDbContextFactory<TContext> innerFactory) : IDbContextFactory<DbContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _innerFactory = innerFactory;

    public DbContext CreateDbContext()
    {
        return _innerFactory.CreateDbContext();
    }
}
