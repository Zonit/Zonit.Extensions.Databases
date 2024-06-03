using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.Abstractions.Repositories;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
public interface IDatabasesRepository<TEntity> : IDisposable
{
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
    /// Update multiple data
    /// </summary>
    /// <param name="predicate">Data to be changed</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
