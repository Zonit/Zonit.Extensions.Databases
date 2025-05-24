using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Zonit.Extensions.Databases.SqlServer.Services;

namespace Zonit.Extensions.Databases.SqlServer.Repositories;

[Obsolete("IDatabasesRepository is deprecated and may be phased out in the future. Use the new IDatabaseRepository solution")]
public abstract class DatabasesRepository<TEntity>(
    ContextBase _context
    ) : IDatabasesRepository<TEntity> 
    where TEntity : class
{
    public List<Expression<Func<TEntity, object?>>>? ExtensionsExpressions { get; set; }
    public List<Expression<Func<TEntity, object>>>? IncludeExpressions { get; set; }
    public Expression<Func<TEntity, bool>>? FilterExpression { get; set; }
    public Expression<Func<TEntity, object>>? OrderByColumnSelector { get; set; }
    public Expression<Func<TEntity, object>>? OrderByDescendingColumnSelector { get; set; }
    public Expression<Func<TEntity, TEntity>>? SelectColumns { get; set; }
    public int? SkipCount { get; set; }
    public int? TakeCount { get; set; }

    public async Task<IReadOnlyCollection<TEntity>?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entitie = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entitie = FilterExpression is not null ? entitie.Where(FilterExpression) : entitie;
        entitie = OrderByColumnSelector is not null ? entitie.OrderBy(OrderByColumnSelector) : entitie;
        entitie = OrderByDescendingColumnSelector is not null ? entitie.OrderByDescending(OrderByDescendingColumnSelector) : entitie;
        entitie = SelectColumns is not null ? entitie.Select(SelectColumns) : entitie;

        if (IncludeExpressions is not null)
            foreach (var includeExpression in IncludeExpressions)
                entitie = entitie.Include(includeExpression);

        entitie = SkipCount is not null ? entitie.Skip(SkipCount.Value) : entitie;
        entitie = TakeCount is not null ? entitie.Take(TakeCount.Value) : entitie;

        var result = await entitie.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (result is null || result.Count == 0)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }

    public async Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entitie = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entitie = FilterExpression is not null ? entitie.Where(FilterExpression) : entitie;
        entitie = OrderByColumnSelector is not null ? entitie.OrderBy(OrderByColumnSelector) : entitie;
        entitie = OrderByDescendingColumnSelector is not null ? entitie.OrderByDescending(OrderByDescendingColumnSelector) : entitie;
        entitie = SelectColumns is not null ? entitie.Select(SelectColumns) : entitie;

        if (IncludeExpressions is not null)
            foreach (var includeExpression in IncludeExpressions)
                entitie = entitie.Include(includeExpression);

        entitie = SkipCount is not null ? entitie.Skip(SkipCount.Value) : entitie;
        entitie = TakeCount is not null ? entitie.Take(TakeCount.Value) : entitie;

        var result = await entitie.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (result is null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }

    public async Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entitie = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        entitie = FilterExpression is not null ? entitie.Where(FilterExpression) : entitie;

        var result = await entitie.ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in result)
        {
            updateAction(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        context.UpdateRange(result);

        var count = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return count > 0 ? count : null;
    }

    public async Task<IReadOnlyCollection<TDto>?> GetAsync<TDto>(CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetAsync(cancellationToken));

    public async Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.GetFirstAsync(cancellationToken));

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _context.LocalDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entitie = context.Set<TEntity>()
            .AsNoTracking();

        if (FilterExpression is not null)
            entitie = entitie.Where(FilterExpression);

        return await entitie.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    private DatabasesRepository<TEntity> Clone()
    {
        var newRepo = (DatabasesRepository<TEntity>)Activator.CreateInstance(this.GetType(), _context)!;

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

    public IDatabasesRepository<TEntity> AsQuery()
        => Clone();

    public IDatabasesRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension)
    {
        var newRepo = Clone();
        newRepo.ExtensionsExpressions ??= [];
        newRepo.ExtensionsExpressions.Add(extension);
        return newRepo;
    }


    public IDatabasesRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression)
    {
        var newRepo = Clone();
        newRepo.IncludeExpressions ??= [];
        newRepo.IncludeExpressions.Add(includeExpression);
        return newRepo;
    }

    public IDatabasesRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector)
    {
        var newRepo = Clone();
        if (newRepo.OrderByDescendingColumnSelector is not null)
            throw new InvalidOperationException("Cannot set both OrderBy and OrderByDescending.");

        newRepo.OrderByColumnSelector = keySelector;
        return newRepo;
    }

    public IDatabasesRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector)
    {
        var newRepo = Clone();
        if (newRepo.OrderByColumnSelector is not null)
            throw new InvalidOperationException("Cannot set both OrderBy and OrderByDescending.");

        newRepo.OrderByDescendingColumnSelector = keySelector;
        return newRepo;
    }

    public IDatabasesRepository<TEntity> Select(Expression<Func<TEntity, TEntity>> selector)
    {
        var newRepo = Clone();
        newRepo.SelectColumns = selector;
        return newRepo;
    }

    public IDatabasesRepository<TEntity> Skip(int skip)
    {
        var newRepo = Clone();
        newRepo.SkipCount = skip;
        return newRepo;
    }

    public IDatabasesRepository<TEntity> Take(int take)
    {
        var newRepo = Clone();
        newRepo.TakeCount = take;
        return newRepo;
    }

    public IDatabasesRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        var newRepo = Clone();

        if (newRepo.FilterExpression is null)
        {
            newRepo.FilterExpression = predicate;
        }
        else
        {
            var invokedExpr = Expression.Invoke(predicate, newRepo.FilterExpression.Parameters.Cast<Expression>());
            newRepo.FilterExpression = Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(newRepo.FilterExpression.Body, invokedExpr),
                newRepo.FilterExpression.Parameters
            );
        }

        return newRepo;
    }
}
