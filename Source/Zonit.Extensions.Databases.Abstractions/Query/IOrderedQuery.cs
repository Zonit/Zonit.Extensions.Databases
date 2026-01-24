using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Query with primary ordering that can add secondary ordering with ThenBy.
/// Prevents OrderBy().OrderBy() - use ThenBy instead.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface IOrderedQuery<TEntity> : IPageableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Adds secondary ascending ordering.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to order by.</typeparam>
    /// <param name="keySelector">Expression to select the ordering key.</param>
    /// <returns>Ordered query for additional ThenBy calls.</returns>
    IOrderedQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

    /// <summary>
    /// Adds secondary descending ordering.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to order by.</typeparam>
    /// <param name="keySelector">Expression to select the ordering key.</param>
    /// <returns>Ordered query for additional ThenBy calls.</returns>
    IOrderedQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
}
