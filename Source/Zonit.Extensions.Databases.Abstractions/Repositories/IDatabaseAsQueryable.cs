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
    new IDatabaseAsQueryable<TEntity> Include(Expression<Func<TEntity, object?>> include);
    new IDatabaseAsQueryable<TEntity> Select<TDto>(Expression<Func<TEntity, TDto>> selector);
    new IDatabaseAsQueryable<TEntity> Skip(int count);
    new IDatabaseAsQueryable<TEntity> Take(int count);
    new IDatabaseAsQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
    new IDatabaseAsQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
}