using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using Zonit.Extensions.Databases.Abstractions.Repositories;
using Zonit.Extensions.Databases.SqlServer.Services;

namespace Zonit.Extensions.Databases.SqlServer.Repositories;

// TODO: Pomyśl by wywalić ID, zrobić na predicate
public abstract class DatabaseRepository<TEntity, TType>(
    DbContext _context,
    IServiceProvider _serviceProvider
    ) : IDatabaseRepository<TEntity, TType> 
    where TEntity : class
{
    public List<Expression<Func<TEntity, object?>>> Extensions { get; set; } = [];

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new DatabaseException($"The {nameof(entity)} parameter cannot be null.");

        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);

        if (await _context.SaveChangesAsync(cancellationToken) > 0 is false)
            throw new DatabaseException("There was a problem when creating record.");

        return entity;
    }

    public async Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto>(await this.AddAsync(entity, cancellationToken));

    public async Task<TEntity?> GetAsync(TType id, CancellationToken cancellationToken = default)
    {
        var property = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (property is null || property.PropertyType != typeof(TType))
            throw new DatabaseException($"Entity's Id property not found or type mismatched with {typeof(TType).Name}.");

        var result = await _context
            .Set<TEntity>()
            .AsNoTracking()
            .Where(x => ((TType)property.GetValue(x)!).Equals(id))
            .SingleOrDefaultAsync(cancellationToken);

        if (result == null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, Extensions, _serviceProvider, cancellationToken);

        return result;
    }

    public async Task<TDto?> GetAsync<TDto>(TType id, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await this.GetAsync(id, cancellationToken));

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await _context
            .Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .SingleOrDefaultAsync(cancellationToken);

        if (result == null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, Extensions, _serviceProvider, cancellationToken);

        return result;
    }

    public async Task<TDto?> GetAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await this.GetAsync(predicate, cancellationToken));

    public async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var result = await _context
            .Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return null;

        result = await ExtensionService.ApplyExtensionsAsync(result, Extensions, _serviceProvider, cancellationToken);

        return result;
    }


    public async Task<TDto?> GetFirstAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => MappingService.Dto<TDto?>(await this.GetFirstAsync(predicate, cancellationToken));

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var property = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (property is null || property.PropertyType != typeof(TType))
            throw new DatabaseException($"Entity's Id property not found or type mismatched with {typeof(TType).Name}.");

        var id = (TType)property.GetValue(entity)!;

        var existingEntity = await _context.Set<TEntity>().FindAsync(id, cancellationToken);

        if (existingEntity is null)
            return false;

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        //_context.Set<T1>().Update(entity);

        if (await _context.SaveChangesAsync(cancellationToken) == 0)
            return false;

        return true;
    }

    public async Task<bool> DeleteAsync(TType id, CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Set<TEntity>()
            .FindAsync(id, cancellationToken);

        if (entity is null)
            return false;

        _context
            .Set<TEntity>()
            .Remove(entity);

        if (await _context.SaveChangesAsync(cancellationToken) == 0)
            return false;

        return true;
    }

    public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return false;

        if (_context.Entry(entity).State == EntityState.Detached)
            _context.Set<TEntity>().Attach(entity);

        _context
            .Set<TEntity>()
            .Remove(entity);

        if (await _context.SaveChangesAsync(cancellationToken) == 0)
            return false;

        return true;
    }

    public IDatabaseReadRepository<TEntity, TType> Extension(Expression<Func<TEntity, object?>> extension)
    {
        var repository = (DatabaseRepository<TEntity, TType>)Activator.CreateInstance(this.GetType(), _context, _serviceProvider)!;

        repository.Extensions.Add(extension);

        return repository;
    }
}