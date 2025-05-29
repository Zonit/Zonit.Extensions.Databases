using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Interfejs do wykonywania operacji zapytań w bazie danych.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IDatabaseQueryOperations<TEntity> :
    IDatabaseEntityOperations<TEntity>,
    IDatabaseQueryable<TEntity>
{
    IDatabaseQueryOperations<TEntity> Include(Expression<Func<TEntity, object?>> includeExpression);
    IDatabaseQueryOperations<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression);
    IDatabaseQueryOperations<TEntity> WhereFullText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);
    IDatabaseQueryOperations<TEntity> WhereFreeText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);
}