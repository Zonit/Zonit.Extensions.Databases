using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.Abstractions.Repositories;

public interface IDatabaseReadRepository<TEntity, TType>
{
    Task<TEntity?> GetAsync(TType id, CancellationToken cancellationToken = default);
    Task<TDto?> GetAsync<TDto>(TType id, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TDto?> GetAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TDto?> GetFirstAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}