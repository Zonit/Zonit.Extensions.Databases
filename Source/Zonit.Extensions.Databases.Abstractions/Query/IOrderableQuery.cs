using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Query that can be ordered with OrderBy/OrderByDescending.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface IOrderableQuery<TEntity> : IPageableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Orders the results ascending by the specified key.
    /// Returns IOrderedQuery for ThenBy chaining.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to order by.</typeparam>
    /// <param name="keySelector">Expression to select the ordering key.</param>
    /// <returns>Ordered query that supports ThenBy.</returns>
    IOrderedQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

    /// <summary>
    /// Orders the results descending by the specified key.
    /// Returns IOrderedQuery for ThenBy chaining.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to order by.</typeparam>
    /// <param name="keySelector">Expression to select the ordering key.</param>
    /// <returns>Ordered query that supports ThenBy.</returns>
    IOrderedQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
}
