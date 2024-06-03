using Zonit.Extensions.Databases.Abstractions.Exceptions;

namespace Zonit.Extensions.Databases.SqlServer.Services;

internal static class MappingService
{
    public static IReadOnlyCollection<TDto>? Dto<TDto>(IEnumerable<object>? entities)
        => entities?.Select(x => Dto<TDto>(x)).ToList();

    public static TDto Dto<TDto>(object? entity)
    {
        if (entity is null)
            return default!;

        var dtoType = typeof(TDto);
        var dtoConstructor = dtoType.GetConstructor([entity.GetType()]);

        if (dtoConstructor is null)
            throw new DatabaseException($"No suitable constructor found for DTO type {dtoType}.");

        return (TDto)dtoConstructor.Invoke([entity]);
    }
}