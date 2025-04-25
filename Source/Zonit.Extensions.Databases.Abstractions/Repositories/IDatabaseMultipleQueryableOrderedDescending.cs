using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

public interface IDatabaseMultipleQueryableOrderedDescending<TEntity> : IDatabaseMultipleRepository<TEntity>
{
    IDatabaseMultipleRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
}