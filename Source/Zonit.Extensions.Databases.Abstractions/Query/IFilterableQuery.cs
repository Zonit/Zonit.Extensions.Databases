using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Query that can be filtered with Where, Include, Extension, and Select.
/// This is the main query building interface.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface IFilterableQuery<TEntity> : IOrderableQuery<TEntity>
    where TEntity : class
{
    // === FILTERING ===

    /// <summary>
    /// Filters the query by the specified predicate.
    /// Multiple Where calls are combined with AND.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Full-text search using SQL Server CONTAINS function.
    /// Requires full-text index on the column.
    /// </summary>
    /// <param name="propertySelector">Expression to select the text property.</param>
    /// <param name="searchTerm">The search term (supports boolean operators).</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> WhereFullText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm);

    /// <summary>
    /// Full-text search using SQL Server FREETEXT function.
    /// Better for natural language queries and word forms.
    /// </summary>
    /// <param name="propertySelector">Expression to select the text property.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> WhereFreeText(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm);

    // === INCLUDES ===

    /// <summary>
    /// Includes a related entity (navigation property).
    /// Returns IIncludableQuery for ThenInclude chaining.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="navigationPropertyPath">Expression to select the navigation property.</param>
    /// <returns>Includable query for ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty?>> navigationPropertyPath);

    /// <summary>
    /// Includes a collection of related entities.
    /// Returns IIncludableQuery for ThenInclude chaining.
    /// </summary>
    /// <typeparam name="TProperty">The element type of the collection.</typeparam>
    /// <param name="navigationPropertyPath">Expression to select the collection property.</param>
    /// <returns>Includable query for ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>?>> navigationPropertyPath);

    // === EXTENSIONS ===

    /// <summary>
    /// Lazy-loads related data from external service via IDatabaseExtension.
    /// Used for loading data from external APIs, microservices, etc.
    /// </summary>
    /// <param name="extensionExpression">Expression to select the extension property.</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression);

    // === PROJECTION ===

    /// <summary>
    /// Projects to a subset of columns (partial select).
    /// </summary>
    /// <param name="selector">Expression to select columns.</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);
}
