using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Zonit.Extensions.Databases.SqlServer.Backgrounds;

internal class MigrationEvent<TContext>(IDbContextFactory<TContext> context) : IHostedService where TContext : DbContext
{
    readonly TContext _context = context.CreateDbContext();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var peding = await _context.Database.GetPendingMigrationsAsync(cancellationToken);

        if (peding.Any() is true)
            await _context.Database.MigrateAsync(cancellationToken);

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
