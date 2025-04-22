namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do usuwania i aktualizacji danych w bazie danych.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseEntityOperations<TEntity>
{
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}