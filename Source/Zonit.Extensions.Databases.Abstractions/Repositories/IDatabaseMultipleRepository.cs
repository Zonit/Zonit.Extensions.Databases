namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do repozytoriów wielu rekordów.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseMultipleRepository<TEntity>
{
    /// <summary>
    /// Returns a list of available results 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of available results by changing them to DTOs
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single result
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single result by changing it to DTO
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update multiple data
    /// </summary>
    /// <param name="predicate">Data to be changed</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the number of available results
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}