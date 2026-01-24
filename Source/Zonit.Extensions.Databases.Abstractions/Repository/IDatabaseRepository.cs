using System.Linq.Expressions;

namespace Zonit.Extensions.Databases;

/// <summary>
/// Main repository interface providing CRUD operations and query building.
/// Use AsQuery() to start building complex queries with fluent API.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository.</typeparam>
public interface IDatabaseRepository<TEntity> where TEntity : class
{
    // === QUERY ENTRY POINT ===

    /// <summary>
    /// Starts building a query with fluent API.
    /// Use this for complex queries with Where, Include, OrderBy, Skip, Take chains.
    /// </summary>
    /// <returns>Filterable query builder.</returns>
    IFilterableQuery<TEntity> AsQuery();

    // === CREATE ===

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added entity with generated values (Id, etc.).</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity and returns it as a DTO.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added entity mapped to DTO.</returns>
    Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the database.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entities added.</returns>
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    // === READ BY ID ===

    /// <summary>
    /// Gets an entity by its integer primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its GUID primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity or null if not found.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its integer primary key and maps to DTO.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The DTO or null if not found.</returns>
    Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its GUID primary key and maps to DTO.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to map to.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The DTO or null if not found.</returns>
    Task<TDto?> GetByIdAsync<TDto>(Guid id, CancellationToken cancellationToken = default);

    // === UPDATE ===

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity by its integer ID using an update action.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="updateAction">Action to apply updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity or null if not found.</returns>
    Task<TEntity?> UpdateAsync(int id, Action<TEntity> updateAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity by its GUID ID using an update action.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="updateAction">Action to apply updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated entity or null if not found.</returns>
    Task<TEntity?> UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an entity (upsert).
    /// If the entity exists by primary key, it's updated. Otherwise, it's inserted.
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upserted entity.</returns>
    Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    // === DELETE ===

    /// <summary>
    /// Deletes an entity. If the entity implements <see cref="ISoftDeletable"/> and <paramref name="forceDelete"/> is false,
    /// performs a soft delete (sets DeletedAt and calls OnSoftDelete). Otherwise, permanently removes the entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="forceDelete">If true, permanently deletes even if entity supports soft delete. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was deleted.</returns>
    Task<bool> DeleteAsync(TEntity entity, bool forceDelete = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its integer ID. If the entity implements <see cref="ISoftDeletable"/> and <paramref name="forceDelete"/> is false,
    /// performs a soft delete (sets DeletedAt and calls OnSoftDelete). Otherwise, permanently removes the entity.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="forceDelete">If true, permanently deletes even if entity supports soft delete. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was deleted.</returns>
    Task<bool> DeleteAsync(int id, bool forceDelete = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its GUID ID. If the entity implements <see cref="ISoftDeletable"/> and <paramref name="forceDelete"/> is false,
    /// performs a soft delete (sets DeletedAt and calls OnSoftDelete). Otherwise, permanently removes the entity.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="forceDelete">If true, permanently deletes even if entity supports soft delete. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was deleted.</returns>
    Task<bool> DeleteAsync(Guid id, bool forceDelete = false, CancellationToken cancellationToken = default);

    // === RESTORE ===

    /// <summary>
    /// Restores a soft-deleted entity by clearing DeletedAt timestamp.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was restored.</returns>
    Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted entity by clearing DeletedAt timestamp.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the entity was restored.</returns>
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken = default);

    // === QUERY METHODS (fluent API) ===

    /// <summary>
    /// Filters entities by a predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>Filterable query for chaining.</returns>
    IFilterableQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Includes a navigation property for eager loading.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="navigationPropertyPath">The navigation property expression.</param>
    /// <returns>Includable query for ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty?>> navigationPropertyPath);

    /// <summary>
    /// Includes a collection navigation property for eager loading.
    /// </summary>
    /// <typeparam name="TProperty">The collection element type.</typeparam>
    /// <param name="navigationPropertyPath">The navigation property expression.</param>
    /// <returns>Includable query for ThenInclude chaining.</returns>
    IIncludableQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> navigationPropertyPath);

    /// <summary>
    /// Lazy-loads related data from external service via IDatabaseExtension.
    /// Used for loading data from external APIs, microservices, etc.
    /// </summary>
    /// <param name="extensionExpression">Expression to select the extension property.</param>
    /// <returns>Filterable query for further chaining.</returns>
    IFilterableQuery<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression);

    /// <summary>
    /// Skips a specified number of entities.
    /// </summary>
    /// <param name="count">Number of entities to skip.</param>
    /// <returns>Skipped query (can only call Take after this).</returns>
    ISkippedQuery<TEntity> Skip(int count);

    /// <summary>
    /// Takes a specified number of entities.
    /// </summary>
    /// <param name="count">Number of entities to take.</param>
    /// <returns>Executable query.</returns>
    IExecutableQuery<TEntity> Take(int count);

    /// <summary>
    /// Orders entities by a key in ascending order.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="keySelector">The key selector expression.</param>
    /// <returns>Ordered query for ThenBy chaining.</returns>
    IOrderedQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

    /// <summary>
    /// Orders entities by a key in descending order.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="keySelector">The key selector expression.</param>
    /// <returns>Ordered query for ThenBy chaining.</returns>
    IOrderedQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);

    /// <summary>
    /// Gets all entities as a list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all entities or null if empty.</returns>
    Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>First entity or null if not found.</returns>
    Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts all entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entities exist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if at least one entity exists.</returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
