using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Abstract base class for query builders.
/// Implements all query interfaces and delegates execution to derived classes.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public abstract class QueryBuilderBase<TEntity> :
    IFilterableQuery<TEntity>,
    IOrderableQuery<TEntity>,
    IOrderedQuery<TEntity>,
    IPageableQuery<TEntity>,
    ISkippedQuery<TEntity>,
    IExecutableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// The immutable query state.
    /// </summary>
    protected QueryState<TEntity> State { get; }

    /// <summary>
    /// Service provider for resolving dependencies (extensions, mappers).
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Mapping service for entity to DTO mapping.
    /// </summary>
    protected IMappingService MappingService { get; }

    /// <summary>
    /// Creates a new query builder with initial state.
    /// </summary>
    protected QueryBuilderBase(QueryState<TEntity> state, IServiceProvider serviceProvider)
    {
        State = state;
        ServiceProvider = serviceProvider;
        MappingService = serviceProvider.GetService<IMappingService>() ?? PassThroughMappingService.Instance;
    }

    /// <summary>
    /// Creates a new query builder instance with the specified state.
    /// Must be implemented by derived classes to return correct type.
    /// </summary>
    protected abstract QueryBuilderBase<TEntity> CreateWithState(QueryState<TEntity> state);

    /// <summary>
    /// Creates an includable query wrapper for ThenInclude support.
    /// Uses IncludableQueryWrapper which delegates back to this query builder.
    /// </summary>
    protected IIncludableQuery<TEntity, TProperty> CreateIncludable<TProperty>(
        QueryState<TEntity> state,
        IncludeInfo currentInclude)
    {
        var builder = CreateWithState(state);
        return new IncludableQueryWrapper<TEntity, TProperty>(builder, currentInclude);
    }

    // === IFilterableQuery ===

    public IFilterableQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return (IFilterableQuery<TEntity>)CreateWithState(State.WithFilter(predicate));
    }

    public IFilterableQuery<TEntity> WhereFullText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(propertySelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        // Build CONTAINS expression - will be translated by provider
        var predicate = BuildFullTextPredicate(propertySelector, searchTerm, useContains: true);
        return (IFilterableQuery<TEntity>)CreateWithState(State.WithFilter(predicate));
    }

    public IFilterableQuery<TEntity> WhereFreeText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(propertySelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        // Build FREETEXT expression - will be translated by provider
        var predicate = BuildFullTextPredicate(propertySelector, searchTerm, useContains: false);
        return (IFilterableQuery<TEntity>)CreateWithState(State.WithFilter(predicate));
    }

    /// <summary>
    /// Builds a full-text search predicate. Override in provider-specific implementation.
    /// </summary>
    protected abstract Expression<Func<TEntity, bool>> BuildFullTextPredicate(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm,
        bool useContains);

    public IIncludableQuery<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty?>> navigationPropertyPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        var include = new IncludeInfo
        {
            Expression = navigationPropertyPath,
            EntityType = typeof(TEntity),
            PropertyType = typeof(TProperty),
            IsCollection = false
        };

        var newState = State.WithInclude(include);
        return CreateIncludable<TProperty>(newState, include);
    }

    public IIncludableQuery<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>?>> navigationPropertyPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        var include = new IncludeInfo
        {
            Expression = navigationPropertyPath,
            EntityType = typeof(TEntity),
            PropertyType = typeof(TProperty),
            IsCollection = true
        };

        var newState = State.WithInclude(include);
        return CreateIncludable<TProperty>(newState, include);
    }

    public IFilterableQuery<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression)
    {
        ArgumentNullException.ThrowIfNull(extensionExpression);
        return (IFilterableQuery<TEntity>)CreateWithState(State.WithExtension(extensionExpression));
    }

    public IFilterableQuery<TEntity> Select(Expression<Func<TEntity, TEntity>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return (IFilterableQuery<TEntity>)CreateWithState(State.WithSelect(selector));
    }

    // === IOrderableQuery ===

    public IOrderedQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var ordering = new OrderingInfo
        {
            Expression = keySelector,
            IsDescending = false,
            IsSecondary = false
        };

        return (IOrderedQuery<TEntity>)CreateWithState(State.WithOrdering(ordering));
    }

    public IOrderedQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var ordering = new OrderingInfo
        {
            Expression = keySelector,
            IsDescending = true,
            IsSecondary = false
        };

        return (IOrderedQuery<TEntity>)CreateWithState(State.WithOrdering(ordering));
    }

    // === IOrderedQuery ===

    public IOrderedQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var ordering = new OrderingInfo
        {
            Expression = keySelector,
            IsDescending = false,
            IsSecondary = true
        };

        return (IOrderedQuery<TEntity>)CreateWithState(State.WithOrdering(ordering));
    }

    public IOrderedQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var ordering = new OrderingInfo
        {
            Expression = keySelector,
            IsDescending = true,
            IsSecondary = true
        };

        return (IOrderedQuery<TEntity>)CreateWithState(State.WithOrdering(ordering));
    }

    // === IPageableQuery ===

    public ISkippedQuery<TEntity> Skip(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Skip count cannot be negative.");

        return (ISkippedQuery<TEntity>)CreateWithState(State.WithSkip(count));
    }

    IExecutableQuery<TEntity> IPageableQuery<TEntity>.Take(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Take count must be positive.");

        return CreateWithState(State.WithTake(count));
    }

    // === ISkippedQuery ===

    IExecutableQuery<TEntity> ISkippedQuery<TEntity>.Take(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Take count must be positive.");

        return CreateWithState(State.WithTake(count));
    }

    // === IExecutableQuery (abstract - provider-specific) ===

    public abstract Task<TEntity?> GetAsync(CancellationToken cancellationToken = default);
    public abstract Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default);
    public abstract Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);
    public abstract Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);
    public abstract Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);
    public abstract Task<IReadOnlyList<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default);
    public abstract Task<IReadOnlyList<TResult>?> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default);
    public abstract Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    public abstract Task<int> CountAsync(CancellationToken cancellationToken = default);
    public abstract Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);
    public abstract Task<int> DeleteRangeAsync(CancellationToken cancellationToken = default);
}
