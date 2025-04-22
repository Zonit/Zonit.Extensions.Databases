namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do pobierania pojedynczego rekordu.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseSingleRepository<TEntity>
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetAsync(CancellationToken cancellationToken = default);
    Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default);
}