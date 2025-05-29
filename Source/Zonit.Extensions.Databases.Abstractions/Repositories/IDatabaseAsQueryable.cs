using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

public interface IDatabaseAsQueryable<TEntity> :
    IDatabaseQueryable<TEntity>,
    IDatabaseQueryOperations<TEntity>,
    IDatabaseMultipleQueryable<TEntity>,
    IDatabaseMultipleRepository<TEntity>,
    IDatabaseSingleRepository<TEntity>,
    IDatabaseMultipleQueryableOrdered<TEntity>,
    IDatabaseMultipleQueryableOrderedDescending<TEntity>

{
    new IDatabaseAsQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    new IDatabaseAsQueryable<TEntity> WhereFullText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);
    new IDatabaseAsQueryable<TEntity> WhereFreeText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);
    new IDatabaseAsQueryable<TEntity> Include(Expression<Func<TEntity, object?>> include);
    new IDatabaseAsQueryable<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);
    new IDatabaseAsQueryable<TEntity> Skip(int count);
    new IDatabaseAsQueryable<TEntity> Take(int count);
    new IDatabaseAsQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
    new IDatabaseAsQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
}