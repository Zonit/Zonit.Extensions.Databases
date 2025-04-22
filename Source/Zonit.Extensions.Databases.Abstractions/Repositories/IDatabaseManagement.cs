namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do zarządzania danymi w bazie danych.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseManagement<TEntity> :
    IDatabaseQueryOperations<TEntity>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);
}