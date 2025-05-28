using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Zonit.Extensions.Databases.SqlServer.Services;

namespace Zonit.Extensions.Databases.SqlServer.Repositories;

public abstract class DatabaseRepository<TEntity>(
        ContextBase _context
    ) : 
        IDatabaseRepository<TEntity>
    where TEntity : class
{
    public List<Expression<Func<TEntity, object?>>>? ExtensionsExpressions { get; set; }
    public List<Expression<Func<TEntity, object?>>>? IncludeExpressions { get; set; }
    public Expression<Func<TEntity, bool>>? FilterExpression { get; set; }
    public Expression<Func<TEntity, object>>? OrderByColumnSelector { get; set; }
    public Expression<Func<TEntity, object>>? OrderByDescendingColumnSelector { get; set; }
    public Expression<Func<TEntity, TEntity>>? SelectColumns { get; set; }
    public int? SkipCount { get; set; }
    public int? TakeCount { get; set; }

    private DatabaseRepository<TEntity> Clone()
    {
        var newRepo = (DatabaseRepository<TEntity>)Activator.CreateInstance(this.GetType(), _context)!;

        newRepo.ExtensionsExpressions = this.ExtensionsExpressions is not null ? [.. this.ExtensionsExpressions] : null;
        newRepo.IncludeExpressions = this.IncludeExpressions is not null ? [.. this.IncludeExpressions] : null;
        newRepo.FilterExpression = this.FilterExpression;
        newRepo.OrderByColumnSelector = this.OrderByColumnSelector;
        newRepo.OrderByDescendingColumnSelector = this.OrderByDescendingColumnSelector;
        newRepo.SelectColumns = this.SelectColumns;
        newRepo.SkipCount = this.SkipCount;
        newRepo.TakeCount = this.TakeCount;

        return newRepo;
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new InvalidOperationException($"The {nameof(entity)} parameter cannot be null.");

        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        await context.Set<TEntity>()
            .AddAsync(entity, cancellationToken);

        if (await context.SaveChangesAsync(cancellationToken) <= 0)
            throw new InvalidOperationException("There was a problem when creating record.");

        return entity;
    }

    public async Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.AddAsync(entity, cancellationToken));

    public IDatabaseAsQueryable<TEntity> AsQuery()
        => new DatabaseAsQueryable<TEntity>(Clone());

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var dbSet = context.Set<TEntity>();
        var entity = await dbSet.FindAsync([id], cancellationToken);

        if (entity is null)
            return false;

        dbSet.Remove(entity);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var dbSet = context.Set<TEntity>();
        var entity = await dbSet.FindAsync([id], cancellationToken);

        if (entity is null)
            return false;

        dbSet.Remove(entity);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return false;

        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        if (context.Entry(entity).State == EntityState.Detached)
            context.Set<TEntity>().Attach(entity);

        context.Set<TEntity>().Remove(entity);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public IDatabaseQueryOperations<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression)
    {
        var newRepo = Clone();
        newRepo.ExtensionsExpressions ??= [];
        newRepo.ExtensionsExpressions.Add(extensionExpression);
        return newRepo;
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await context.Set<TEntity>().AnyAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(CancellationToken cancellationToken = default)
        => await GetFirstAsync(cancellationToken);

    public async Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await GetAsync(cancellationToken));

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken);

        if (entity is null)
            return null;

        // Dodajemy Include jeśli są zdefiniowane
        if (IncludeExpressions != null && IncludeExpressions.Count > 0)
        {
            // Ładujemy powiązane encje z Include
            foreach (var includeExpression in IncludeExpressions)
            {
                await context.Entry(entity).Reference(includeExpression).LoadAsync(cancellationToken);
            }
        }

        entity = await ExtensionService.ApplyExtensionsAsync(entity, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return entity;
    }

    public async Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetByIdAsync(id, cancellationToken));

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken);

        if (entity is null)
            return null;

        // Dodajemy Include jeśli są zdefiniowane
        if (IncludeExpressions != null && IncludeExpressions.Count > 0)
        {
            // Ładujemy powiązane encje z Include
            foreach (var includeExpression in IncludeExpressions)
            {
                await context.Entry(entity).Reference(includeExpression).LoadAsync(cancellationToken);
            }
        }

        entity = await ExtensionService.ApplyExtensionsAsync(entity, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return entity;
    }

    public async Task<TDto?> GetByIdAsync<TDto>(Guid id, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetByIdAsync(id, cancellationToken));

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = context.Set<TEntity>()
            .AsNoTracking();

        if (FilterExpression is not null)
            entities = entities.Where(FilterExpression);

        return await entities.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entities = FilterExpression is not null ? entities.Where(FilterExpression) : entities;
        entities = OrderByColumnSelector is not null ? entities.OrderBy(OrderByColumnSelector) : entities;
        entities = OrderByDescendingColumnSelector is not null ? entities.OrderByDescending(OrderByDescendingColumnSelector) : entities;
        entities = SelectColumns is not null ? entities.Select(SelectColumns) : entities;

        if (IncludeExpressions is not null)
            foreach (var includeExpression in IncludeExpressions)
                entities = entities.Include(includeExpression);

        entities = SkipCount is not null ? entities.Skip(SkipCount.Value) : entities;
        entities = TakeCount is not null ? entities.Take(TakeCount.Value) : entities;

        var result = await entities.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (result is null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }

    public async Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetFirstAsync(cancellationToken));

    public async Task<IReadOnlyCollection<TEntity>?> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entities = FilterExpression is not null ? entities.Where(FilterExpression) : entities;
        entities = OrderByColumnSelector is not null ? entities.OrderBy(OrderByColumnSelector) : entities;
        entities = OrderByDescendingColumnSelector is not null ? entities.OrderByDescending(OrderByDescendingColumnSelector) : entities;
        entities = SelectColumns is not null ? entities.Select(SelectColumns) : entities;

        if (IncludeExpressions is not null)
            foreach (var includeExpression in IncludeExpressions)
                entities = entities.Include(includeExpression);

        entities = SkipCount is not null ? entities.Skip(SkipCount.Value) : entities;
        entities = TakeCount is not null ? entities.Take(TakeCount.Value) : entities;

        var result = await entities.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (result is null || result.Count == 0)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetListAsync(cancellationToken));

    public IDatabaseQueryOperations<TEntity> Include(Expression<Func<TEntity, object?>> includeExpression)
    {
        var newRepo = Clone();
        newRepo.IncludeExpressions ??= [];
        newRepo.IncludeExpressions.Add(includeExpression);
        return newRepo;
    }

    public IDatabaseMultipleRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector)
    {
        var newRepo = Clone();
        if (newRepo.OrderByDescendingColumnSelector is not null)
            throw new InvalidOperationException("Cannot set both OrderBy and OrderByDescending.");

        newRepo.OrderByColumnSelector = keySelector;
        return newRepo;
    }

    public IDatabaseMultipleRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector)
    {
        var newRepo = Clone();
        if (newRepo.OrderByColumnSelector is not null)
            throw new InvalidOperationException("Cannot set both OrderBy and OrderByDescending.");

        newRepo.OrderByDescendingColumnSelector = keySelector;
        return newRepo;
    }

    public IDatabaseQueryOperations<TEntity> Select(Expression<Func<TEntity, TEntity>> selector)
    {
        var newRepo = Clone();
        newRepo.SelectColumns = selector;
        return newRepo;
    }

    public IDatabaseMultipleQueryable<TEntity> Skip(int count)
    {
        var newRepo = Clone();
        newRepo.SkipCount = count;
        return newRepo;
    }

    public IDatabaseMultipleQueryable<TEntity> Take(int count)
    {
        var newRepo = Clone();
        newRepo.TakeCount = count;
        return newRepo;
    }

    public async Task<bool> UpdateAsync(int id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity is null)
            return false;

        updateAction(entity);

        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = await context.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity is null)
            return false;

        updateAction(entity);

        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return false;

        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        context.Entry(entity).State = EntityState.Modified;

        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entities = FilterExpression is not null ? entities.Where(FilterExpression) : entities;

        var result = await entities.ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in result)
        {
            updateAction(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        context.UpdateRange(result);

        var count = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return count > 0 ? count : null;
    }

    public IDatabaseQueryOperations<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression)
    {
        var newRepo = Clone();

        if (newRepo.FilterExpression is null)
        {
            newRepo.FilterExpression = whereExpression;
        }
        else
        {
            var invokedExpr = Expression.Invoke(whereExpression, newRepo.FilterExpression.Parameters.Cast<Expression>());
            newRepo.FilterExpression = Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(newRepo.FilterExpression.Body, invokedExpr),
                newRepo.FilterExpression.Parameters
            );
        }

        return newRepo;
    }
}
