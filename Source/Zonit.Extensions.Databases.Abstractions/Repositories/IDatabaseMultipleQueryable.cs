namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do budowania zapytania wielu rekordów
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseMultipleQueryable<TEntity> :
    IDatabaseMultipleQueryableOrdered<TEntity>,
    IDatabaseMultipleQueryableOrderedDescending<TEntity>
{
    IDatabaseMultipleQueryable<TEntity> Skip(int count);
    IDatabaseMultipleQueryable<TEntity> Take(int count);
}