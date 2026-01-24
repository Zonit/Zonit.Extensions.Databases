using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Query with pending Include that can chain ThenInclude for nested navigation properties.
/// Follows EF Core's Include/ThenInclude pattern.
/// </summary>
/// <typeparam name="TEntity">The root entity type being queried.</typeparam>
/// <typeparam name="TProperty">The type of the included navigation property.</typeparam>
public interface IIncludableQuery<TEntity, TProperty> : IFilterableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Includes a nested navigation property from the previously included entity.
    /// </summary>
    /// <typeparam name="TNext">The type of the nested navigation property.</typeparam>
    /// <param name="navigationPropertyPath">Expression to select the nested property.</param>
    /// <returns>Includable query for further ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, TNext?>> navigationPropertyPath);

    /// <summary>
    /// Includes a nested collection navigation property from the previously included entity.
    /// </summary>
    /// <typeparam name="TNext">The element type of the nested collection.</typeparam>
    /// <param name="navigationPropertyPath">Expression to select the nested collection.</param>
    /// <returns>Includable query for further ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, IEnumerable<TNext>?>> navigationPropertyPath);
}
