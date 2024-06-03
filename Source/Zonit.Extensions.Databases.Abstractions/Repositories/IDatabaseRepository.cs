using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.Abstractions.Repositories;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
/// <typeparam name="TType">ID Type</typeparam>
public interface IDatabaseRepository<TEntity, TType>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(TType id, CancellationToken cancellationToken = default);
    Task<TDto?> GetAsync<TDto>(TType id, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TDto?> GetAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TDto?> GetFirstAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(TType entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}