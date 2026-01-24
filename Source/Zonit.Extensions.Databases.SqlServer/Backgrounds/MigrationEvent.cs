using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Zonit.Extensions.Databases.SqlServer.Backgrounds;

/// <summary>
/// Hosted service that performs database migration on application startup.
/// Optimized for fast startup with lazy context creation and parallel-safe execution.
/// </summary>
/// <typeparam name="TContext">The DbContext type to migrate.</typeparam>
/// <remarks>
/// EF Core migrations are not supported with NativeAOT. Use migration bundles for AOT scenarios.
/// </remarks>
[RequiresDynamicCode("EF Core migrations require dynamic code generation.")]
internal sealed class MigrationEvent<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory;
    private readonly ILogger<MigrationEvent<TContext>>? _logger;
    private static volatile bool _migrationCompleted;
    private static readonly object _migrationLock = new();

    public MigrationEvent(
        IDbContextFactory<TContext> contextFactory,
        ILogger<MigrationEvent<TContext>>? logger = null)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Skip if already migrated (fast path for app restarts)
        if (_migrationCompleted)
            return;

        try
        {
            // Use lock to prevent concurrent migrations of same context type
            lock (_migrationLock)
            {
                if (_migrationCompleted)
                    return;
            }

            await using var context = await _contextFactory.CreateDbContextAsync(stoppingToken);

            var pending = await context.Database.GetPendingMigrationsAsync(stoppingToken);
            var pendingList = pending.ToList();

            if (pendingList.Count > 0)
            {
                _logger?.LogInformation(
                    "Applying {Count} pending migrations for {Context}...",
                    pendingList.Count,
                    typeof(TContext).Name);

                await context.Database.MigrateAsync(stoppingToken);

                _logger?.LogInformation(
                    "Successfully applied migrations for {Context}",
                    typeof(TContext).Name);
            }

            _migrationCompleted = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to migrate database for {Context}", typeof(TContext).Name);
            throw;
        }
    }
}

/// <summary>
/// Coordinator service that runs all database migrations in parallel.
/// Register this once instead of individual MigrationEvent services for faster startup.
/// </summary>
/// <remarks>
/// EF Core migrations are not supported with NativeAOT. Use migration bundles for AOT scenarios.
/// </remarks>
[RequiresDynamicCode("EF Core migrations require dynamic code generation.")]
internal sealed class ParallelMigrationCoordinator : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ParallelMigrationCoordinator>? _logger;
    private readonly List<Func<CancellationToken, Task>> _migrationTasks = [];

    public ParallelMigrationCoordinator(
        IServiceProvider serviceProvider,
        ILogger<ParallelMigrationCoordinator>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registers a migration task for a specific context type.
    /// </summary>
    internal void RegisterMigration<TContext>() where TContext : DbContext
    {
        _migrationTasks.Add(async (ct) =>
        {
            var factory = _serviceProvider.GetService(typeof(IDbContextFactory<TContext>)) as IDbContextFactory<TContext>;
            if (factory is null)
                return;

            await using var context = await factory.CreateDbContextAsync(ct);
            var pending = await context.Database.GetPendingMigrationsAsync(ct);

            if (pending.Any())
            {
                _logger?.LogInformation("Migrating {Context}...", typeof(TContext).Name);
                await context.Database.MigrateAsync(ct);
            }
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_migrationTasks.Count == 0)
            return;

        _logger?.LogInformation("Starting parallel migration of {Count} contexts...", _migrationTasks.Count);

        try
        {
            // Run all migrations in parallel
            await Task.WhenAll(_migrationTasks.Select(t => t(stoppingToken)));

            _logger?.LogInformation("All database migrations completed successfully.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "One or more database migrations failed.");
            throw;
        }
    }
}
