using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Wspólny interfejs do budowania zapytania dla pojedynczego rekordu oraz wielu rekordów.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseQueryable<TEntity> :
    IDatabaseSingleRepository<TEntity>, // repozytorium pojedyńczego rekordu
    IDatabaseMultipleQueryable<TEntity>, // zapytanie wielu rekordów
    IDatabaseMultipleRepository<TEntity> // repozytorium wielu rekordów
{
    IDatabaseQueryOperations<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression);
    IDatabaseQueryOperations<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);
}