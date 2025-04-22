using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

public interface IDatabaseMultipleQueryableOrderedDescending<TEntity> : IDatabaseMultipleRepository<TEntity>
{
    IDatabaseMultipleRepository<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);
}