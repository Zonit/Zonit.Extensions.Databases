﻿using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
[Obsolete("IDatabasesRepository is deprecated and may be phased out in the future. Use the new IDatabaseRepository solution")]
public interface IDatabasesRepository<TEntity>
{
    IDatabasesRepository<TEntity> AsQuery();
    IDatabasesRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension);
    IDatabasesRepository<TEntity> Skip(int skip);
    IDatabasesRepository<TEntity> Take(int take);
    IDatabasesRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression);
    IDatabasesRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    IDatabasesRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
    IDatabasesRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
    IDatabasesRepository<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);

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