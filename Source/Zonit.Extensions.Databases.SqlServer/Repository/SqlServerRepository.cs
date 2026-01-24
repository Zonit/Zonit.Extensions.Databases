using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.SqlServer;

/// <summary>
/// SQL Server implementation of the database repository.
/// Provides CRUD operations and fluent query building for Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
/// <remarks>
/// This class uses Entity Framework Core which requires dynamic code generation.
/// Native AOT compilation may not work correctly with all features.
/// </remarks>
[RequiresUnreferencedCode("EF Core isn't fully compatible with trimming.")]
[RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT.")]
public class SqlServerRepository<TEntity> : IDatabaseRepository<TEntity>
    where TEntity : class
{
    private readonly IDbContextFactory<DbContext> _contextFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMappingService _mappingService;

    /// <summary>
    /// Empty service provider used when no service provider is available.
    /// </summary>
    private static readonly IServiceProvider EmptyServiceProvider = new ServiceCollection().BuildServiceProvider();

    /// <summary>
    /// Gets the DbContext factory for creating database contexts.
    /// Use this when you need direct access to create contexts in derived repositories.
    /// </summary>
    protected IDbContextFactory<DbContext> ContextFactory => _contextFactory;

    /// <summary>
    /// Creates a new database context asynchronously.
    /// This is a convenience method for derived repositories that need direct database access.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new DbContext instance. Remember to dispose it (use await using).</returns>
    protected ValueTask<DbContext> CreateContextAsync(CancellationToken cancellationToken = default)
        => new(_contextFactory.CreateDbContextAsync(cancellationToken));

    /// <summary>
    /// Creates a new SQL Server repository.
    /// </summary>
    /// <param name="contextFactory">The EF Core DbContext factory.</param>
    /// <param name="serviceProvider">Optional service provider for extension resolution.</param>
    public SqlServerRepository(IDbContextFactory<DbContext> contextFactory, IServiceProvider? serviceProvider = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _serviceProvider = serviceProvider ?? EmptyServiceProvider;
        _mappingService = serviceProvider?.GetService<IMappingService>() ?? PassThroughMappingService.Instance;
    }

    /// <summary>
    /// Creates a new SQL Server repository with an adapter factory.
    /// This is used internally by derived classes with specific DbContext types.
    /// </summary>
    protected SqlServerRepository(
        IDbContextFactory<DbContext> contextFactory,
        IServiceProvider? serviceProvider,
        bool _)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _serviceProvider = serviceProvider ?? EmptyServiceProvider;
        _mappingService = serviceProvider?.GetService<IMappingService>() ?? PassThroughMappingService.Instance;
    }

    // === QUERY ENTRY POINT ===

    /// <inheritdoc />
    public IFilterableQuery<TEntity> AsQuery()
    {
        var state = QueryState<TEntity>.Empty;
        return new SqlServerQueryBuilder<TEntity>(state, _serviceProvider, _contextFactory);
    }

    // === CREATE ===

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entry = await context.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default)
    {
        var added = await AddAsync(entity, cancellationToken);
        return _mappingService.Map<TDto>(added)
               ?? throw new InvalidOperationException($"Failed to map entity to {typeof(TDto).Name}");
    }

    /// <inheritdoc />
    public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        await context.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // === READ BY ID ===

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return _mappingService.Map<TDto>(entity);
    }

    /// <inheritdoc />
    public async Task<TDto?> GetByIdAsync<TDto>(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return _mappingService.Map<TDto>(entity);
    }

    // === UPDATE ===

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity?> UpdateAsync(int id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);

        if (entity is null)
            return null;

        updateAction(entity);
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity?> UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);

        if (entity is null)
            return null;

        updateAction(entity);
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Get primary key values
        var keyValues = context.Model.FindEntityType(typeof(TEntity))?
            .FindPrimaryKey()?
            .Properties
            .Select(p => p.PropertyInfo?.GetValue(entity))
            .ToArray();

        if (keyValues is null || keyValues.Length == 0)
            throw new InvalidOperationException($"Cannot determine primary key for entity {typeof(TEntity).Name}");

        var existing = await context.Set<TEntity>().FindAsync(keyValues, cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            context.Entry(existing).CurrentValues.SetValues(entity);
        }
        else
        {
            await context.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    // === DELETE ===

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(TEntity entity, bool forceDelete = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Soft delete if entity supports it and forceDelete is false
        if (!forceDelete && entity is ISoftDeletable softDeletable)
        {
            softDeletable.DeletedAt = DateTimeOffset.UtcNow;
            softDeletable.OnSoftDelete();
            context.Entry(entity).State = EntityState.Modified;
            var softAffected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return softAffected > 0;
        }

        // Hard delete
        context.Set<TEntity>().Remove(entity);
        var affected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, bool forceDelete = false, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);

        if (entity is null)
            return false;

        // Soft delete if entity supports it and forceDelete is false
        if (!forceDelete && entity is ISoftDeletable softDeletable)
        {
            softDeletable.DeletedAt = DateTimeOffset.UtcNow;
            softDeletable.OnSoftDelete();
            context.Entry(entity).State = EntityState.Modified;
            var softAffected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return softAffected > 0;
        }

        // Hard delete
        context.Set<TEntity>().Remove(entity);
        var affected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, bool forceDelete = false, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken).ConfigureAwait(false);

        if (entity is null)
            return false;

        // Soft delete if entity supports it and forceDelete is false
        if (!forceDelete && entity is ISoftDeletable softDeletable)
        {
            softDeletable.DeletedAt = DateTimeOffset.UtcNow;
            softDeletable.OnSoftDelete();
            context.Entry(entity).State = EntityState.Modified;
            var softAffected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return softAffected > 0;
        }

        // Hard delete
        context.Set<TEntity>().Remove(entity);
        var affected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Need to ignore query filters to find soft-deleted entities
        var entity = await context.Set<TEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return false;

        if (entity is not ISoftDeletable softDeletable)
        {
            throw new InvalidOperationException(
                $"Entity {typeof(TEntity).Name} does not implement ISoftDeletable.");
        }

        if (!softDeletable.DeletedAt.HasValue)
            return false; // Not deleted

        softDeletable.DeletedAt = null;
        context.Entry(entity).State = EntityState.Modified;

        var affected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Need to ignore query filters to find soft-deleted entities
        var entity = await context.Set<TEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return false;

        if (entity is not ISoftDeletable softDeletable)
        {
            throw new InvalidOperationException(
                $"Entity {typeof(TEntity).Name} does not implement ISoftDeletable.");
        }

        if (!softDeletable.DeletedAt.HasValue)
            return false; // Not deleted

        softDeletable.DeletedAt = null;
        context.Entry(entity).State = EntityState.Modified;

        var affected = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }

    // === QUERY METHODS (fluent API) ===

    /// <inheritdoc />
    public IFilterableQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        return AsQuery().Where(predicate);
    }

    /// <inheritdoc />
    public IIncludableQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty?>> navigationPropertyPath)
    {
        return AsQuery().Include(navigationPropertyPath);
    }

    /// <inheritdoc />
    public IIncludableQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> navigationPropertyPath)
    {
        return AsQuery().Include(navigationPropertyPath);
    }

    /// <inheritdoc />
    public IFilterableQuery<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression)
    {
        return AsQuery().Extension(extensionExpression);
    }

    /// <inheritdoc />
    public ISkippedQuery<TEntity> Skip(int count)
    {
        return AsQuery().Skip(count);
    }

    /// <inheritdoc />
    public IExecutableQuery<TEntity> Take(int count)
    {
        return AsQuery().Take(count);
    }

    /// <inheritdoc />
    public IOrderedQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        return AsQuery().OrderBy(keySelector);
    }

    /// <inheritdoc />
    public IOrderedQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        return AsQuery().OrderByDescending(keySelector);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default)
    {
        return AsQuery().GetListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        return AsQuery().GetFirstAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return AsQuery().CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return AsQuery().AnyAsync(cancellationToken);
    }
}
