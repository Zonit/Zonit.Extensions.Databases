namespace Zonit.Extensions.Databases;

/// <summary>
/// Service for mapping entities to DTOs.
/// Implement this interface with Source Generator for AOT support.
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// Maps an entity to a DTO.
    /// </summary>
    /// <typeparam name="TDto">The target DTO type.</typeparam>
    /// <param name="entity">The source entity.</param>
    /// <returns>The mapped DTO or null if entity is null.</returns>
    TDto? Map<TDto>(object? entity);

    /// <summary>
    /// Maps a list of entities to DTOs.
    /// </summary>
    /// <typeparam name="TDto">The target DTO type.</typeparam>
    /// <param name="entities">The source entities.</param>
    /// <returns>The mapped DTOs or null if entities is null/empty.</returns>
    IReadOnlyList<TDto>? MapList<TDto>(IEnumerable<object>? entities);
}

/// <summary>
/// Default mapping service that returns entities as-is.
/// Register your own IMappingService for actual DTO mapping.
/// </summary>
public sealed class PassThroughMappingService : IMappingService
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static IMappingService Instance { get; } = new PassThroughMappingService();

    /// <inheritdoc />
    public TDto? Map<TDto>(object? entity)
    {
        if (entity is null)
            return default;

        if (entity is TDto dto)
            return dto;

        throw new InvalidOperationException(
            $"Cannot map {entity.GetType().Name} to {typeof(TDto).Name}. " +
            "Register an IMappingService implementation for DTO mapping.");
    }

    /// <inheritdoc />
    public IReadOnlyList<TDto>? MapList<TDto>(IEnumerable<object>? entities)
    {
        if (entities is null)
            return null;

        if (entities is IReadOnlyList<TDto> dtoList)
            return dtoList;

        if (entities is IEnumerable<TDto> dtoEnumerable)
            return dtoEnumerable.ToList();

        throw new InvalidOperationException(
            $"Cannot map entities to {typeof(TDto).Name}. " +
            "Register an IMappingService implementation for DTO mapping.");
    }
}
