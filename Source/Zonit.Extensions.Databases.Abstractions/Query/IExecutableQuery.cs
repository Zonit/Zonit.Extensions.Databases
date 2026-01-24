using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Query ready for execution with terminal operations.
/// This is the final state of a query builder chain.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface IExecutableQuery<TEntity> where TEntity : class
{
    // === SINGLE RESULT ===

    /// <summary>
    /// Gets a single entity matching the query, or null if not found.
    /// Equivalent to FirstOrDefault.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity or null.</returns>
    Task<TEntity?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity mapped to DTO, or null if not found.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The DTO or null.</returns>
    Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the query, or null if not found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first entity or null.</returns>
    Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity mapped to DTO, or null if not found.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first DTO or null.</returns>
    Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

    // === MULTIPLE RESULTS ===

    /// <summary>
    /// Gets all entities matching the query.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entities, or null if none found.</returns>
    Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities mapped to DTOs.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of DTOs, or null if none found.</returns>
    Task<IReadOnlyList<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default);

    // === ANONYMOUS PROJECTION ===

    /// <summary>
    /// Projects entities to an anonymous type or custom projection.
    /// </summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="selector">The projection expression.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projected results.</returns>
    Task<IReadOnlyList<TResult>?> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default);

    // === AGGREGATES ===

    /// <summary>
    /// Checks if any entities match the query.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if at least one entity matches.</returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the query.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of matching entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    // === BULK OPERATIONS ===

    /// <summary>
    /// Updates all entities matching the query.
    /// </summary>
    /// <param name="updateAction">Action to apply to each entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of updated entities, or null if none.</returns>
    Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all entities matching the query.
    /// Uses EF Core ExecuteDeleteAsync for bulk delete.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of deleted entities.</returns>
    Task<int> DeleteRangeAsync(CancellationToken cancellationToken = default);
}
