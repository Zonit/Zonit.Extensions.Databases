using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Zonit.Extensions.Databases;

namespace Zonit.Extensions.Databases.Abstractions.Repositories
{
    public interface IDatabaseRepository<TEntity> :
        IDatabaseQueryable<TEntity>,            // Buodowanie zapytania
        IDatabaseSingleRepository<TEntity>,     // Repozytorium pojedynczego rekordu
        IDatabaseMultipleQueryable<TEntity>,    // Repozytorium wielu rekordów
        IDatabaseManagement<TEntity>            // Zarządzanie danymi
    {
        IDatabaseRepository<TEntity> AsQuery();
    }

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

    /// <summary>
    /// Interfejs do wykonywania operacji zapytań w bazie danych.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IDatabaseQueryOperations<TEntity> : 
        IDatabaseEntityOperations<TEntity>,
        IDatabaseQueryable<TEntity>
    {
        IDatabaseQueryOperations<TEntity> Include<TKey>(Expression<Func<TEntity, TKey>> includeExpression);
        IDatabaseQueryOperations<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression);
    }

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

    /// <summary>
    /// Wspólny interfejs do budowania zapytania dla pojedynczego rekordu oraz wielu rekordów.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IDatabaseQueryable<TEntity> :
        IDatabaseSingleRepository<TEntity>, // repozytorium pojedyńczego rekordu
        IDatabaseMultipleQueryable<TEntity>, // zapytanie wielu rekordów
        IDatabaseMultipleRepository<TEntity> // repozytorium wielu rekordów
    {
        IDatabaseQueryable<TEntity> Extension<TKey>(Expression<Func<TEntity, TKey?>> extensionExpression);
        IDatabaseQueryable<TEntity> Select<TDto>(Expression<Func<TEntity, TDto>> selector);
    }

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

    public interface IDatabaseMultipleQueryableOrdered<TEntity> : IDatabaseMultipleRepository<TEntity>
    {
        IDatabaseMultipleRepository<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
    }

    public interface IDatabaseMultipleQueryableOrderedDescending<TEntity> : IDatabaseMultipleRepository<TEntity>
    {
        IDatabaseMultipleRepository<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
    }

    /// <summary>
    /// Interfejs do repozytoriów wielu rekordów.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IDatabaseMultipleRepository<TEntity>
    {
        /// <summary>
        /// Returns a list of available results 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of available results by changing them to DTOs
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single result
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single result by changing it to DTO
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Update multiple data
        /// </summary>
        /// <param name="predicate">Data to be changed</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the number of available results
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    }


    public class Test123
    {

        //public async Task Test123123()
        //{
        //    IDatabaseRepository<DatabaseOptions> test = null;

        //    var dupa = test.Include(x => x.Parameters).Select(x => x.Name).Extension(x => x.Parameters).GetAsync();

        //    var dupa2 = test
        //        .Where(x => x.Parameters == "")
        //        .Skip(1)
        //        .Take(2)
        //        .OrderBy(x => x.Parameters)
        //        .GetListAsync();

        //    var dupa3 = test
        //        .Include(x => x.Parameters)
        //        .Where(x => x.Parameters == "dupa")
        //        .UpdateAsync(new DatabaseOptions(), CancellationToken.None);

        //    var dupaaaa = test.Where(x => x.Raw == "").OrderBy(x => x.User).GetListAsync();

        //}
    }
}

namespace Dupa 
{
    // Root - główny entrypoint
    public interface IDatabaseRepository<TEntity>
        : IUpdateRoot<TEntity>, IDeleteRoot<TEntity>, IQueryRoot<TEntity>
    {
        IDatabaseRepository<TEntity> AsQuery();
    }

    // ============ UPDATE =============

    // Root interfejs, tylko tu zaczynamy update flow
    public interface IUpdateRoot<TEntity>
    {
        IUpdateWhere<TEntity> Include<TKey>(Expression<Func<TEntity, TKey>> includeExpression);
    }

    // Po Include mamy Where
    public interface IUpdateWhere<TEntity>
    {
        IUpdateDo<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    }

    // Po Where dozwolone tylko Update
    public interface IUpdateDo<TEntity>
    {
        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        // TODO: Możesz dodać UpdateRange jak chcesz
    }

    // ============ DELETE =============

    // Root interfejs, tylko tu zaczynamy delete flow
    public interface IDeleteRoot<TEntity>
    {
        IDeleteWhere<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    }

    // Po Where dozwolone tylko Delete
    public interface IDeleteDo<TEntity>
    {
        Task<bool> DeleteAsync(CancellationToken cancellationToken = default);
    }

    // IDeleteWhere dziedziczy po akcji Delete
    public interface IDeleteWhere<TEntity> : IDeleteDo<TEntity>
    {
    }

    // ============= ODCZYTY ============

    // Główny interfejs odczytu z zapytaniami
    public interface IQueryRoot<TEntity> :
        IQueryInclude<TEntity>,
        ISimpleGet<TEntity>
    { }

    // Budowanie zapytania: Include
    public interface IQueryInclude<TEntity>
    {
        IQueryWhere<TEntity> Include<TKey>(Expression<Func<TEntity, TKey>> includeExpression);
        IQueryWhere<TEntity> Extension<TKey>(Expression<Func<TEntity, TKey?>> extensionExpression);
    }

    // Następne: Where
    public interface IQueryWhere<TEntity>
    {
        IQueryOrder<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    }

    // Po Where -- wybieranie sortowania
    public interface IQueryOrder<TEntity>
    {
        IQueryOrdered<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IQueryOrderedDesc<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IQueryTakeSkip<TEntity> NoOrder(); // Jeśli nie chcesz sortowania, od razu do Take/Skip
    }

    // Po jednym z sortowań NIE MOŻESZ użyć kolejnego
    public interface IQueryOrdered<TEntity> : IQueryTakeSkip<TEntity> { }
    public interface IQueryOrderedDesc<TEntity> : IQueryTakeSkip<TEntity> { }

    // Po Where (lub sortowaniu) dajesz Take, Skip, Select, GetAsync/gety
    public interface IQueryTakeSkip<TEntity> : IQueryExecute<TEntity>
    {
        IQueryTakeSkip<TEntity> Skip(int n);
        IQueryTakeSkip<TEntity> Take(int n);
        IQuerySelect<TEntity> Select<TDto>(Expression<Func<TEntity, TDto>> selector);
    }

    // Select daje możliwość dalszego getowania DTO
    public interface IQuerySelect<TEntity> : IQueryExecute<TEntity>
    {
    }

    // Metody kończące
    public interface IQueryExecute<TEntity>
    {
        Task<IReadOnlyCollection<TEntity>> GetAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetFirstAsync(CancellationToken cancellationToken = default);
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    }

    // Prosty dostęp do Get po samym ID bez budowania zapytania
    public interface ISimpleGet<TEntity>
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class Test123
    {

        //public async Task Test123123()
        //{
        //    IDatabaseRepository<DatabaseOptions> test = null;

        //    var dupa = test.Where(x => x.Parameters == "").DeleteAsync();

        //    test.Include(x => x.Parameters)
        //        .Where(x => x.Parameters == "dupa")
                
        //        .Select(x => x.Name)
        //        .Extension(x => x.Parameters)
        //        .OrderByDescending(x => x.Parameters)
        //        .OrderBy(x => x.Parameters)
        //        .Skip(1)
        //        .Take(2)
        //        .GetAsync();


        //    test.OrderByDescending(x => x.Parameters)
        //        .OrderBy(x => x.Parameters)
        //        .Skip(1)
        //        .Take(2)
        //        .Where(x => x.Parameters == "dupa")
        //        .GetAsync();

        //}
    }
}