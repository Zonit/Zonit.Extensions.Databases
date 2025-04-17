using System.Linq.Expressions;
using Zonit.Extensions.Databases.Abstractions.Repositories;

namespace Zonit.Extensions.Databases;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
/// <typeparam name="TType">ID Type</typeparam>
public interface IDatabaseRepository<TEntity> : IDatabaseReadRepository<TEntity>
{
    IDatabaseReadRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}