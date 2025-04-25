using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

public interface IDatabaseMultipleQueryableOrdered<TEntity> : IDatabaseMultipleRepository<TEntity>
{
    IDatabaseMultipleRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
    //IDatabaseMultipleRepository<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
}