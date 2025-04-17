using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Model name</typeparam>
public interface IDatabasesRepository<TEntity> : IDatabasesReadRepository<TEntity>
{
    IDatabasesRepository<TEntity> Query();
    IDatabasesRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension);
    IDatabasesRepository<TEntity> Skip(int skip);
    IDatabasesRepository<TEntity> Take(int take);
    IDatabasesRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression);
    IDatabasesRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    IDatabasesRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
    IDatabasesRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
    IDatabasesRepository<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);
}

