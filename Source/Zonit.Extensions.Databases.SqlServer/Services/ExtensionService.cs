using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Zonit.Extensions.Databases.Accessors;

namespace Zonit.Extensions.Databases.SqlServer.Services;

/// <summary>
/// AOT-safe extension service for loading external data via IDatabaseExtension.
/// </summary>
internal static class ExtensionService
{
    /// <summary>
    /// Cache for extension service keys to avoid repeated key generation.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string> _extensionKeyCache = new();

    public static async Task<TEntity> ApplyExtensionsAsync<TEntity>(
        TEntity entity,
        IEnumerable<Expression<Func<TEntity, object?>>>? extensions,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (extensions == null || !extensions.Any())
            return entity;

        await ProcessExtensionsAsync(entity, extensions, serviceProvider, cancellationToken);
        return entity;
    }

    public static async Task<List<TEntity>> ApplyExtensionsAsync<TEntity>(
        List<TEntity> entities,
        IEnumerable<Expression<Func<TEntity, object?>>>? extensions,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (extensions == null || !extensions.Any())
            return entities;

        foreach (var entity in entities)
        {
            await ProcessExtensionsAsync(entity, extensions, serviceProvider, cancellationToken);
        }

        return entities;
    }

    private static async Task ProcessExtensionsAsync<TEntity>(
        TEntity entity,
        IEnumerable<Expression<Func<TEntity, object?>>> extensions,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        // AOT-safe: Use generated accessor instead of GetProperty reflection
        var accessor = EntityAccessorRegistry.GetAccessor<TEntity>();

        foreach (var extension in extensions)
        {
            var memberExpression = GetMemberExpression(extension);
            if (memberExpression == null)
                continue;

            var propertyName = memberExpression.Member.Name;
            var propertyInfo = accessor?.GetPropertyInfo(propertyName);
            if (propertyInfo == null)
                continue;

            var currentValue = accessor?.GetValue(entity, propertyName);
            if (currentValue != null)
                continue;

            var foreignKeyName = propertyName + "Id";
            var foreignKeyValue = accessor?.GetValue(entity, foreignKeyName);
            if (foreignKeyValue is not Guid idValue || idValue == Guid.Empty)
                continue;

            // AOT-safe: Use keyed service pattern or non-generic interface
            // First try keyed service (preferred for AOT)
            var extensionKey = GetExtensionKey(propertyInfo.PropertyType);

            await using var scope = serviceProvider.CreateAsyncScope();

            // Try to resolve via non-generic IDatabaseExtension with keyed service
            var extensionService = scope.ServiceProvider.GetKeyedService<IDatabaseExtension>(extensionKey);
            if (extensionService == null)
            {
                // AOT-safe: Use generated ExtensionTypeResolver instead of GetInterfaces
                var allExtensions = scope.ServiceProvider.GetServices<IDatabaseExtension>();
                extensionService = ExtensionTypeResolver.FindExtension(allExtensions, propertyInfo.PropertyType);
            }

            if (extensionService == null)
                continue;

            var loadedValue = await extensionService.InitializeAsync(idValue, cancellationToken);
            if (loadedValue != null)
            {
                accessor?.SetValue(entity, propertyName, loadedValue);
            }
        }
    }

    /// <summary>
    /// Gets the extension service key for a property type.
    /// </summary>
    private static string GetExtensionKey(Type propertyType)
    {
        return _extensionKeyCache.GetOrAdd(propertyType, t => $"Extension:{t.FullName}");
    }

    private static MemberExpression? GetMemberExpression<TEntity>(Expression<Func<TEntity, object?>> expression)
    {
        return expression.Body switch
        {
            MemberExpression mExp => mExp,
            UnaryExpression uExp when uExp.Operand is MemberExpression mExp => mExp,
            _ => null
        };
    }
}
