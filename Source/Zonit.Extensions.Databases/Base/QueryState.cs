using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Stores the state of include operations for query building.
/// Supports nested ThenInclude chains.
/// </summary>
public sealed class IncludeInfo
{
    /// <summary>
    /// The expression representing the navigation property to include.
    /// </summary>
    public required LambdaExpression Expression { get; init; }

    /// <summary>
    /// The type of entity this include starts from.
    /// </summary>
    public required Type EntityType { get; init; }

    /// <summary>
    /// The type of the navigation property being included.
    /// </summary>
    public required Type PropertyType { get; init; }

    /// <summary>
    /// Whether this is a collection navigation property.
    /// </summary>
    public bool IsCollection { get; init; }

    /// <summary>
    /// Nested ThenInclude operations.
    /// </summary>
    public List<IncludeInfo> ThenIncludes { get; } = [];
}

/// <summary>
/// Stores ordering information for query building.
/// </summary>
public sealed class OrderingInfo
{
    /// <summary>
    /// The expression to order by.
    /// </summary>
    public required LambdaExpression Expression { get; init; }

    /// <summary>
    /// Whether to order descending.
    /// </summary>
    public bool IsDescending { get; init; }

    /// <summary>
    /// Whether this is a secondary ordering (ThenBy vs OrderBy).
    /// </summary>
    public bool IsSecondary { get; init; }
}

/// <summary>
/// Immutable state container for query builder.
/// Used to implement fluent API without mutation.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public sealed record QueryState<TEntity> where TEntity : class
{
    /// <summary>
    /// Empty query state with default values.
    /// </summary>
    public static QueryState<TEntity> Empty { get; } = new();

    /// <summary>
    /// Combined filter expressions (ANDed together).
    /// </summary>
    public Expression<Func<TEntity, bool>>? Filter { get; init; }

    /// <summary>
    /// List of include operations with nested ThenIncludes.
    /// </summary>
    public IReadOnlyList<IncludeInfo> Includes { get; init; } = [];

    /// <summary>
    /// List of extension expressions for lazy loading external data.
    /// </summary>
    public IReadOnlyList<Expression<Func<TEntity, object?>>> Extensions { get; init; } = [];

    /// <summary>
    /// Column projection expression.
    /// </summary>
    public Expression<Func<TEntity, TEntity>>? Select { get; init; }

    /// <summary>
    /// List of ordering operations.
    /// </summary>
    public IReadOnlyList<OrderingInfo> Orderings { get; init; } = [];

    /// <summary>
    /// Number of records to skip.
    /// </summary>
    public int? Skip { get; init; }

    /// <summary>
    /// Number of records to take.
    /// </summary>
    public int? Take { get; init; }

    /// <summary>
    /// Creates a new state with updated filter.
    /// </summary>
    public QueryState<TEntity> WithFilter(Expression<Func<TEntity, bool>> predicate)
    {
        var combined = Filter is null
            ? predicate
            : CombineExpressions(Filter, predicate);

        return this with { Filter = combined };
    }

    /// <summary>
    /// Creates a new state with added include.
    /// </summary>
    public QueryState<TEntity> WithInclude(IncludeInfo include)
    {
        var newIncludes = new List<IncludeInfo>(Includes) { include };
        return this with { Includes = newIncludes };
    }

    /// <summary>
    /// Creates a new state with added extension.
    /// </summary>
    public QueryState<TEntity> WithExtension(Expression<Func<TEntity, object?>> extension)
    {
        var newExtensions = new List<Expression<Func<TEntity, object?>>>(Extensions) { extension };
        return this with { Extensions = newExtensions };
    }

    /// <summary>
    /// Creates a new state with select projection.
    /// </summary>
    public QueryState<TEntity> WithSelect(Expression<Func<TEntity, TEntity>> selector)
    {
        return this with { Select = selector };
    }

    /// <summary>
    /// Creates a new state with added ordering.
    /// </summary>
    public QueryState<TEntity> WithOrdering(OrderingInfo ordering)
    {
        var newOrderings = new List<OrderingInfo>(Orderings) { ordering };
        return this with { Orderings = newOrderings };
    }

    /// <summary>
    /// Creates a new state with skip value.
    /// </summary>
    public QueryState<TEntity> WithSkip(int count)
    {
        return this with { Skip = count };
    }

    /// <summary>
    /// Creates a new state with take value.
    /// </summary>
    public QueryState<TEntity> WithTake(int count)
    {
        return this with { Take = count };
    }

    /// <summary>
    /// Combines two filter expressions with AND.
    /// </summary>
    private static Expression<Func<TEntity, bool>> CombineExpressions(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");

        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

        var combined = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
    }

    /// <summary>
    /// Replaces a parameter in an expression.
    /// </summary>
    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(expression);
    }

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParam ? newParam : base.VisitParameter(node);
        }
    }
}
