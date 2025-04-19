using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using Zonit.Extensions.Databases.SqlServer.Services;

namespace Zonit.Extensions.Databases.SqlServer.Repositories;

// TODO: Pomyśl by wywalić ID, zrobić na predicate
public abstract class DatabaseRepository<TEntity>(
    ContextBase _context
    ) : IDatabaseRepository<TEntity> 
    where TEntity : class
{
    public List<Expression<Func<TEntity, object?>>>? ExtensionsExpressions { get; set; }
    public List<Expression<Func<TEntity, object>>>? IncludeExpressions { get; set; }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new DatabaseException($"The {nameof(entity)} parameter cannot be null.");

        await _context.DbContext
            .Set<TEntity>()
            .AddAsync(entity, cancellationToken);

        if (await _context.DbContext.SaveChangesAsync(cancellationToken) > 0 is false)
            throw new DatabaseException("There was a problem when creating record.");

        return entity;
    }

    public async Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.AddAsync(entity, cancellationToken));

    private async Task<TEntity?> GetAsyncInternal<TId>(TId id, CancellationToken cancellationToken = default)
    {
        var property = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (property is null || property.PropertyType != typeof(TId))
            throw new DatabaseException($"Entity's Id property not found or type mismatched with {typeof(TId).Name}.");

        var result = await _context.DbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(x => ((TId)property.GetValue(x)!).Equals(id))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result is null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    public async Task<TEntity?> GetAsync(int id, CancellationToken cancellationToken = default)
        => await GetAsyncInternal(id, cancellationToken);

    public async Task<TDto?> GetAsync<TDto>(int id, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await GetAsync(id, cancellationToken).ConfigureAwait(false));

    public async Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await GetAsyncInternal(id, cancellationToken);

    public async Task<TDto?> GetAsync<TDto>(Guid id, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await GetAsync(id, cancellationToken).ConfigureAwait(false));


    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await _context.DbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .SingleOrDefaultAsync(cancellationToken);

        if (result == null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }

    public async Task<TDto?> GetAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await this.GetAsync(predicate, cancellationToken));

    public async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await _context.DbContext
            .Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, ExtensionsExpressions, _context.ServiceProvider, cancellationToken);

        return result;
    }


    public async Task<TDto?> GetFirstAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await this.GetFirstAsync(predicate, cancellationToken));

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var property = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
            throw new DatabaseException("Entity's Id property not found.");

        if (property.PropertyType != typeof(int) && property.PropertyType != typeof(Guid))
            throw new DatabaseException($"Entity's Id property must be of type int or Guid but is {property.PropertyType.Name}.");

        var id = property.GetValue(entity)!;
        var existingEntity = await _context.DbContext
            .Set<TEntity>()
            .FindAsync(new object[] { id }, cancellationToken);

        if (existingEntity is null)
            return false;

        _context.DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);

        if (await _context.DbContext.SaveChangesAsync(cancellationToken) == 0)
            return false;

        return true;
    }


    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbSet = _context.DbContext.Set<TEntity>();
        var entity = await dbSet.FindAsync([id], cancellationToken);
        if (entity is null)
            return false;

        dbSet.Remove(entity);
        return await _context.DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbSet = _context.DbContext.Set<TEntity>();
        var entity = await dbSet.FindAsync([id], cancellationToken);
        if (entity is null)
            return false;

        dbSet.Remove(entity);
        return await _context.DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return false;

        if (_context.DbContext.Entry(entity).State == EntityState.Detached)
            _context.DbContext.Set<TEntity>().Attach(entity);

        _context.DbContext.Set<TEntity>().Remove(entity);
        return await _context.DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public IDatabaseRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression)
    {
        var newRepo = Clone();
        newRepo.IncludeExpressions ??= [];
        newRepo.IncludeExpressions.Add(includeExpression);
        return newRepo;
    }

    public IDatabaseRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension)
    {
        var newRepo = Clone();
        newRepo.ExtensionsExpressions ??= [];
        newRepo.ExtensionsExpressions.Add(extension);
        return newRepo;
    }

    private DatabaseRepository<TEntity> Clone()
    {
        var newRepo = (DatabaseRepository<TEntity>)Activator.CreateInstance(this.GetType(), _context)!;

        newRepo.ExtensionsExpressions = this.ExtensionsExpressions is not null ? [.. this.ExtensionsExpressions] : null;
        newRepo.IncludeExpressions = this.IncludeExpressions is not null ? [.. this.IncludeExpressions] : null;

        return newRepo;
    }
}