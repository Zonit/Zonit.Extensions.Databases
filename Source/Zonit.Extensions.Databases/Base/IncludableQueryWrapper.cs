using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Wrapper that provides ThenInclude support by delegating to the underlying query builder.
/// </summary>
/// <typeparam name="TEntity">The root entity type.</typeparam>
/// <typeparam name="TProperty">The type of the included property.</typeparam>
internal sealed class IncludableQueryWrapper<TEntity, TProperty> : IIncludableQuery<TEntity, TProperty>
    where TEntity : class
{
    private readonly QueryBuilderBase<TEntity> _queryBuilder;
    private readonly IncludeInfo _currentInclude;

    /// <summary>
    /// Creates a new includable query wrapper.
    /// </summary>
    public IncludableQueryWrapper(QueryBuilderBase<TEntity> queryBuilder, IncludeInfo currentInclude)
    {
        _queryBuilder = queryBuilder;
        _currentInclude = currentInclude;
    }

    // === IIncludableQuery (ThenInclude support) ===

    public IIncludableQuery<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, TNext?>> navigationPropertyPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        var thenInclude = new IncludeInfo
        {
            Expression = navigationPropertyPath,
            EntityType = typeof(TProperty),
            PropertyType = typeof(TNext),
            IsCollection = false
        };

        _currentInclude.ThenIncludes.Add(thenInclude);

        return new IncludableQueryWrapper<TEntity, TNext>(_queryBuilder, thenInclude);
    }

    public IIncludableQuery<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, IEnumerable<TNext>?>> navigationPropertyPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        var thenInclude = new IncludeInfo
        {
            Expression = navigationPropertyPath,
            EntityType = typeof(TProperty),
            PropertyType = typeof(TNext),
            IsCollection = true
        };

        _currentInclude.ThenIncludes.Add(thenInclude);

        return new IncludableQueryWrapper<TEntity, TNext>(_queryBuilder, thenInclude);
    }

    // === IFilterableQuery (delegates to underlying builder) ===

    public IFilterableQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => _queryBuilder.Where(predicate);

    public IFilterableQuery<TEntity> WhereFullText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm)
        => _queryBuilder.WhereFullText(propertySelector, searchTerm);

    public IFilterableQuery<TEntity> WhereFreeText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm)
        => _queryBuilder.WhereFreeText(propertySelector, searchTerm);

    public IIncludableQuery<TEntity, T> Include<T>(
        Expression<Func<TEntity, T?>> navigationPropertyPath)
        => _queryBuilder.Include(navigationPropertyPath);

    public IIncludableQuery<TEntity, T> Include<T>(
        Expression<Func<TEntity, IEnumerable<T>?>> navigationPropertyPath)
        => _queryBuilder.Include(navigationPropertyPath);

    public IFilterableQuery<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression)
        => _queryBuilder.Extension(extensionExpression);

    public IFilterableQuery<TEntity> Select(Expression<Func<TEntity, TEntity>> selector)
        => _queryBuilder.Select(selector);

    // === IOrderableQuery (delegates to underlying builder) ===

    public IOrderedQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => _queryBuilder.OrderBy(keySelector);

    public IOrderedQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => _queryBuilder.OrderByDescending(keySelector);

    // === IPageableQuery (delegates to underlying builder) ===

    public ISkippedQuery<TEntity> Skip(int count)
        => _queryBuilder.Skip(count);

    public IExecutableQuery<TEntity> Take(int count)
        => ((IPageableQuery<TEntity>)_queryBuilder).Take(count);

    // === IExecutableQuery (delegates to underlying builder) ===

    public Task<TEntity?> GetAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.GetAsync(cancellationToken);

    public Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default)
        => _queryBuilder.GetAsync<TDto>(cancellationToken);

    public Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.GetFirstAsync(cancellationToken);

    public Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default)
        => _queryBuilder.GetFirstAsync<TDto>(cancellationToken);

    public Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.GetListAsync(cancellationToken);

    public Task<IReadOnlyList<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default)
        => _queryBuilder.GetListAsync<TDto>(cancellationToken);

    public Task<IReadOnlyList<TResult>?> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default)
        => _queryBuilder.SelectAsync(selector, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.AnyAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.CountAsync(cancellationToken);

    public Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
        => _queryBuilder.UpdateRangeAsync(updateAction, cancellationToken);

    public Task<int> DeleteRangeAsync(CancellationToken cancellationToken = default)
        => _queryBuilder.DeleteRangeAsync(cancellationToken);
}
