using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.SqlServer.Repositories;

public class DatabaseAsQueryable<TEntity>(
        DatabaseRepository<TEntity> _repository
    ) : IDatabaseAsQueryable<TEntity>
    where TEntity : class
{
    #region IDatabaseAsQueryable Implementation

    public IDatabaseAsQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Where(predicate));

    public IDatabaseAsQueryable<TEntity> Include(Expression<Func<TEntity, object?>> include)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Include(include));

    public IDatabaseAsQueryable<TEntity> Select(Expression<Func<TEntity, TEntity>> selector)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Select(selector));

    public IDatabaseAsQueryable<TEntity> Skip(int count)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Skip(count));

    public IDatabaseAsQueryable<TEntity> Take(int count)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Take(count));

    public IDatabaseAsQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.OrderBy(keySelector));

    public IDatabaseAsQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.OrderByDescending(keySelector));

    #endregion

    #region IDatabaseQueryOperations Implementation

    public IDatabaseQueryOperations<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression)
        => new DatabaseAsQueryable<TEntity>((DatabaseRepository<TEntity>)_repository.Extension(extensionExpression));

    IDatabaseQueryOperations<TEntity> IDatabaseQueryOperations<TEntity>.Include(Expression<Func<TEntity, object?>> includeExpression)
        => Include(includeExpression);

    IDatabaseQueryOperations<TEntity> IDatabaseQueryOperations<TEntity>.Where(Expression<Func<TEntity, bool>> whereExpression)
        => Where(whereExpression);

    IDatabaseQueryOperations<TEntity> IDatabaseQueryable<TEntity>.Select(Expression<Func<TEntity, TEntity>> selector)
        => Select(selector);

    #endregion

    #region IDatabaseMultipleQueryable Implementation

    IDatabaseMultipleQueryable<TEntity> IDatabaseMultipleQueryable<TEntity>.Skip(int count)
        => Skip(count);

    IDatabaseMultipleQueryable<TEntity> IDatabaseMultipleQueryable<TEntity>.Take(int count)
        => Take(count);

    #endregion

    #region IDatabaseMultipleQueryableOrdered Implementation

    IDatabaseMultipleRepository<TEntity> IDatabaseMultipleQueryableOrdered<TEntity>.OrderBy(Expression<Func<TEntity, object>> keySelector)
        => OrderBy(keySelector);

    #endregion

    #region IDatabaseMultipleQueryableOrderedDescending Implementation

    IDatabaseMultipleRepository<TEntity> IDatabaseMultipleQueryableOrderedDescending<TEntity>.OrderByDescending(Expression<Func<TEntity, object>> keySelector)
        => OrderByDescending(keySelector);

    #endregion

    #region Query Execution Methods

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => _repository.AnyAsync(cancellationToken);

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        => _repository.GetCountAsync(cancellationToken);

    public Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
        => _repository.GetFirstAsync(cancellationToken);

    public Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default)
        => _repository.GetFirstAsync<TDto>(cancellationToken);

    public Task<IReadOnlyCollection<TEntity>?> GetListAsync(CancellationToken cancellationToken = default)
        => _repository.GetListAsync(cancellationToken);

    public Task<IReadOnlyCollection<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default)
        => _repository.GetListAsync<TDto>(cancellationToken);

    #endregion

    #region Single Repository Methods

    public Task<TEntity?> GetAsync(CancellationToken cancellationToken = default)
        => _repository.GetAsync(cancellationToken);

    public Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default)
        => _repository.GetAsync<TDto>(cancellationToken);

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync<TDto>(id, cancellationToken);

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public Task<TDto?> GetByIdAsync<TDto>(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync<TDto>(id, cancellationToken);

    #endregion

    #region Update Methods

    public Task<bool> UpdateAsync(int id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
        => _repository.UpdateAsync(id, updateAction, cancellationToken);

    public Task<bool> UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
        => _repository.UpdateAsync(id, updateAction, cancellationToken);

    public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _repository.UpdateAsync(entity, cancellationToken);

    public Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
        => _repository.UpdateRangeAsync(updateAction, cancellationToken);

    #endregion

    #region Delete Methods

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(entity, cancellationToken);

    #endregion
}