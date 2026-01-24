namespace Zonit.Extensions.Databases.Accessors;

/// <summary>
/// AOT-safe registry mapping entity types to their extension service types.
/// Extended by source-generated partial class with actual extension registrations.
/// </summary>
public static partial class ExtensionTypeResolver
{
    /// <summary>
    /// Gets the extension service type for the specified entity type.
    /// Returns null if no extension was generated for this type.
    /// </summary>
    public static Type? GetExtensionType(Type entityType)
    {
        return GetGeneratedExtensionType(entityType);
    }

    /// <summary>
    /// Gets the entity type for the specified extension service type.
    /// </summary>
    public static Type? GetEntityType(Type extensionType)
    {
        return GetGeneratedEntityType(extensionType);
    }

    /// <summary>
    /// Checks if the given extension service handles the specified entity type.
    /// </summary>
    public static bool HandlesEntity(IDatabaseExtension extension, Type entityType)
    {
        return HandlesEntityGenerated(extension, entityType);
    }

    /// <summary>
    /// Finds an extension service that handles the specified entity type.
    /// </summary>
    public static IDatabaseExtension? FindExtension(IEnumerable<IDatabaseExtension> extensions, Type entityType)
    {
        return FindExtensionGenerated(extensions, entityType);
    }

    // Partial methods for source-generated implementation
    static partial void GetGeneratedExtensionTypeCore(Type entityType, ref Type? result);
    static partial void GetGeneratedEntityTypeCore(Type extensionType, ref Type? result);
    static partial void HandlesEntityGeneratedCore(IDatabaseExtension extension, Type entityType, ref bool result);
    static partial void FindExtensionGeneratedCore(IEnumerable<IDatabaseExtension> extensions, Type entityType, ref IDatabaseExtension? result);

    private static Type? GetGeneratedExtensionType(Type entityType)
    {
        Type? result = null;
        GetGeneratedExtensionTypeCore(entityType, ref result);
        return result;
    }

    private static Type? GetGeneratedEntityType(Type extensionType)
    {
        Type? result = null;
        GetGeneratedEntityTypeCore(extensionType, ref result);
        return result;
    }

    private static bool HandlesEntityGenerated(IDatabaseExtension extension, Type entityType)
    {
        bool result = false;
        HandlesEntityGeneratedCore(extension, entityType, ref result);

        // Fallback: use reflection when source generator is not available
        if (!result)
        {
            result = HandlesEntityWithReflection(extension, entityType);
        }

        return result;
    }

    private static IDatabaseExtension? FindExtensionGenerated(IEnumerable<IDatabaseExtension> extensions, Type entityType)
    {
        IDatabaseExtension? result = null;
        FindExtensionGeneratedCore(extensions, entityType, ref result);

        // Fallback: use reflection when source generator is not available
        if (result is null)
        {
            foreach (var ext in extensions)
            {
                if (HandlesEntityWithReflection(ext, entityType))
                {
                    result = ext;
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Reflection-based fallback for checking if extension handles entity type.
    /// Used when source generator is not available (e.g., project references).
    /// </summary>
#pragma warning disable IL2075 // GetInterfaces() is safe here - extension types are registered at startup
    private static bool HandlesEntityWithReflection(IDatabaseExtension extension, Type entityType)
    {
        var extType = extension.GetType();
        foreach (var iface in extType.GetInterfaces())
        {
            if (iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IDatabaseExtension<>) &&
                iface.GetGenericArguments()[0] == entityType)
            {
                return true;
            }
        }
        return false;
    }
#pragma warning restore IL2075
}
