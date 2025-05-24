namespace Zonit.Extensions.Databases;

public interface IDatabaseRepository<TEntity> :
    IDatabaseQueryable<TEntity>,            // Buodowanie zapytania
    IDatabaseSingleRepository<TEntity>,     // Repozytorium pojedynczego rekordu
    IDatabaseMultipleQueryable<TEntity>,    // Repozytorium wielu rekordów
    IDatabaseManagement<TEntity>            // Zarządzanie danymi
{
    IDatabaseAsQueryable<TEntity> AsQuery();
}