﻿namespace Zonit.Extensions.Databases;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
public interface IDatabasesReadRepository<TEntity>
{
    /// <summary>
    /// Returns a list of available results 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<TEntity>?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of available results by changing them to DTOs
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<TDto>?> GetAsync<TDto>(CancellationToken cancellationToken = default);

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
