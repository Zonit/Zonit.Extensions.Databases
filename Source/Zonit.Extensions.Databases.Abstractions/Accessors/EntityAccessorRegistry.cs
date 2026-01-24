using System.Collections.Frozen;
using System.Reflection;

namespace Zonit.Extensions.Databases.Accessors;

/// <summary>
/// AOT-safe registry of all entity accessors.
/// Extended by source-generated partial class with actual accessor registrations.
/// </summary>
public static partial class EntityAccessorRegistry
{
    /// <summary>
    /// Gets the accessor for the specified entity type.
    /// Returns null if no accessor was generated for this type.
    /// </summary>
    /// <remarks>
    /// This method is extended by source-generated code.
    /// If no [DatabaseEntity] attributes are found, returns null.
    /// </remarks>
    public static IEntityAccessor? GetAccessor(Type entityType)
    {
        return GetGeneratedAccessor(entityType);
    }

    /// <summary>
    /// Gets the typed accessor for the specified entity type.
    /// </summary>
    public static IEntityAccessor<TEntity>? GetAccessor<TEntity>() where TEntity : class
    {
        return GetGeneratedAccessor(typeof(TEntity)) as IEntityAccessor<TEntity>;
    }

    /// <summary>
    /// Placeholder for source-generated implementation.
    /// Returns null when no [DatabaseEntity] classes are found.
    /// </summary>
    static partial void GetGeneratedAccessorCore(Type entityType, ref IEntityAccessor? result);

    private static IEntityAccessor? GetGeneratedAccessor(Type entityType)
    {
        IEntityAccessor? result = null;
        GetGeneratedAccessorCore(entityType, ref result);
        return result;
    }
}
