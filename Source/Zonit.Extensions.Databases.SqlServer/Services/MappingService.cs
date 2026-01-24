using System.Diagnostics.CodeAnalysis;

namespace Zonit.Extensions.Databases.SqlServer.Services;

/// <summary>
/// Internal mapping service that uses constructor-based DTO creation.
/// </summary>
/// <remarks>
/// This service uses reflection to find and invoke constructors.
/// Consider using a source generator or AutoMapper for AOT compatibility.
/// </remarks>
internal static class MappingService
{
    [RequiresUnreferencedCode("DTO mapping uses reflection to find constructors.")]
    [RequiresDynamicCode("DTO mapping uses reflection to invoke constructors.")]
    public static IReadOnlyCollection<TDto>? Dto<TDto>(IEnumerable<object>? entities)
        => entities?.Select(x => Dto<TDto>(x)).ToList();

    [RequiresUnreferencedCode("DTO mapping uses reflection to find constructors.")]
    [RequiresDynamicCode("DTO mapping uses reflection to invoke constructors.")]
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