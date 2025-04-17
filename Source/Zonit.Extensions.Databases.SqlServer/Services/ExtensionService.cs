using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Zonit.Extensions.Databases.SqlServer.Services;

internal static class ExtensionService
{
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
        foreach (var extension in extensions)
        {
            var memberExpression = GetMemberExpression(extension);
            if (memberExpression == null)
                continue;

            var propertyName = memberExpression.Member.Name;
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            if (propertyInfo == null || propertyInfo.GetValue(entity) != null)
                continue;

            var foreignKeyName = propertyName + "Id";
            var foreignKeyProperty = typeof(TEntity).GetProperty(foreignKeyName);
            if (foreignKeyProperty == null)
                continue;

            var foreignKeyValue = foreignKeyProperty.GetValue(entity);
            if (foreignKeyValue is not Guid idValue || idValue == Guid.Empty)
                continue;

            var extensionInterfaceType = typeof(IDatabaseExtension<>).MakeGenericType(propertyInfo.PropertyType);

            using var scope = serviceProvider.CreateAsyncScope();
            var extensionService = scope.ServiceProvider.GetService(extensionInterfaceType);
            if (extensionService == null)
                continue;

            dynamic ext = extensionService;
            var loadedValue = await ext.InicjalizeAsync(idValue, cancellationToken);
            propertyInfo.SetValue(entity, loadedValue);
        }
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
