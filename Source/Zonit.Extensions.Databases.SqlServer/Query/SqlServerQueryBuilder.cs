using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Zonit.Extensions.Databases.Accessors;

namespace Zonit.Extensions.Databases.SqlServer;

/// <summary>
/// SQL Server implementation of the query builder.
/// Translates query state to EF Core queries with SQL Server-specific features.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
/// <remarks>
/// This class uses Entity Framework Core and expression tree compilation which requires dynamic code generation.
/// Native AOT compilation may not work correctly with all features.
/// </remarks>
[RequiresUnreferencedCode("EF Core and expression trees require unreferenced code.")]
[RequiresDynamicCode("EF Core and expression trees require dynamic code generation.")]
public sealed class SqlServerQueryBuilder<TEntity> : QueryBuilderBase<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Cached MethodInfo for SqlServerDbFunctionsExtensions.FreeText - initialized once at class load.
    /// </summary>
    private static readonly MethodInfo FreeTextMethod = typeof(SqlServerDbFunctionsExtensions)
        .GetMethod(
            nameof(SqlServerDbFunctionsExtensions.FreeText),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

    /// <summary>
    /// Cached MethodInfo for SqlServerDbFunctionsExtensions.Contains - initialized once at class load.
    /// </summary>
    private static readonly MethodInfo ContainsMethod = typeof(SqlServerDbFunctionsExtensions)
        .GetMethod(
            nameof(SqlServerDbFunctionsExtensions.Contains),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private readonly IDbContextFactory<DbContext> _contextFactory;

    /// <summary>
    /// Creates a new SQL Server query builder.
    /// </summary>
    public SqlServerQueryBuilder(
        QueryState<TEntity> state,
        IServiceProvider serviceProvider,
        IDbContextFactory<DbContext> contextFactory)
        : base(state, serviceProvider)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    protected override QueryBuilderBase<TEntity> CreateWithState(QueryState<TEntity> state)
    {
        return new SqlServerQueryBuilder<TEntity>(state, ServiceProvider, _contextFactory);
    }

    /// <inheritdoc />
    protected override Expression<Func<TEntity, bool>> BuildFullTextPredicate(
        Expression<Func<TEntity, string>> propertySelector,
        string searchTerm,
        bool useContains)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Invoke(propertySelector, parameter);

        // Use cached MethodInfo instead of runtime reflection for AOT compatibility
        var method = useContains ? ContainsMethod : FreeTextMethod;

        var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));

        var call = Expression.Call(
            method,
            efFunctions,
            property,
            Expression.Constant(searchTerm));

        return Expression.Lambda<Func<TEntity, bool>>(call, parameter);
    }

    /// <summary>
    /// Builds and executes a query.
    /// </summary>
    private async Task<IQueryable<TEntity>> BuildQueryAsync(CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<TEntity> query = context.Set<TEntity>()
            .AsSplitQuery()
            .AsNoTracking();

        // Apply filters
        if (State.Filter is not null)
        {
            query = query.Where(State.Filter);
        }

        // Apply includes
        query = ApplyIncludes(query);

        // Apply orderings
        query = ApplyOrderings(query);

        // Apply select projection
        if (State.Select is not null)
        {
            query = query.Select(State.Select);
        }

        // Apply pagination
        if (State.Skip.HasValue)
        {
            query = query.Skip(State.Skip.Value);
        }

        if (State.Take.HasValue)
        {
            query = query.Take(State.Take.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies include operations to the query.
    /// </summary>
    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
    {
        foreach (var include in State.Includes)
        {
            query = ApplyIncludeRecursive(query, include);
        }
        return query;
    }

    /// <summary>
    /// Recursively applies Include and ThenInclude using AOT-safe expression trees.
    /// </summary>
    private IQueryable<TEntity> ApplyIncludeRecursive(IQueryable<TEntity> query, IncludeInfo include)
    {
        // AOT-safe Include using Expression.Call
        var includeCall = Expression.Call(
            typeof(EntityFrameworkQueryableExtensions),
            nameof(EntityFrameworkQueryableExtensions.Include),
            [typeof(TEntity), include.PropertyType],
            query.Expression,
            Expression.Quote(include.Expression));

        var includableQuery = query.Provider.CreateQuery<TEntity>(includeCall);

        // Apply ThenIncludes recursively using expression tree manipulation
        foreach (var thenInclude in include.ThenIncludes)
        {
            includableQuery = ApplyThenIncludeAot(includableQuery, include, thenInclude);
        }

        return includableQuery;
    }

    /// <summary>
    /// AOT-safe ThenInclude using Expression.Call.
    /// </summary>
    private static IQueryable<TEntity> ApplyThenIncludeAot(
        IQueryable<TEntity> query,
        IncludeInfo parentInclude,
        IncludeInfo thenInclude)
    {
        // Determine which ThenInclude overload to use based on whether parent is a collection
        var methodName = nameof(EntityFrameworkQueryableExtensions.ThenInclude);

        // Build the IIncludableQueryable type for the query
        Type includableType = parentInclude.IsCollection
            ? typeof(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<,>)
                .MakeGenericType(typeof(TEntity), typeof(IEnumerable<>).MakeGenericType(parentInclude.PropertyType))
            : typeof(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<,>)
                .MakeGenericType(typeof(TEntity), parentInclude.PropertyType);

        var thenIncludeCall = Expression.Call(
            typeof(EntityFrameworkQueryableExtensions),
            methodName,
            parentInclude.IsCollection
                ? [typeof(TEntity), parentInclude.PropertyType, thenInclude.PropertyType]
                : [typeof(TEntity), parentInclude.PropertyType, thenInclude.PropertyType],
            query.Expression,
            Expression.Quote(thenInclude.Expression));

        return query.Provider.CreateQuery<TEntity>(thenIncludeCall);
    }

    /// <summary>
    /// Applies ordering operations to the query using AOT-safe expression trees.
    /// </summary>
    private IQueryable<TEntity> ApplyOrderings(IQueryable<TEntity> query)
    {
        IOrderedQueryable<TEntity>? orderedQuery = null;

        foreach (var ordering in State.Orderings)
        {
            var keySelector = ordering.Expression;

            if (orderedQuery is null || !ordering.IsSecondary)
            {
                // First OrderBy
                orderedQuery = ordering.IsDescending
                    ? ApplyOrderByDescendingAot(query, keySelector)
                    : ApplyOrderByAot(query, keySelector);
            }
            else
            {
                // ThenBy
                orderedQuery = ordering.IsDescending
                    ? ApplyThenByDescendingAot(orderedQuery, keySelector)
                    : ApplyThenByAot(orderedQuery, keySelector);
            }
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// AOT-safe OrderBy using Expression.Call instead of MakeGenericMethod.
    /// </summary>
    private static IOrderedQueryable<TEntity> ApplyOrderByAot(IQueryable<TEntity> query, LambdaExpression keySelector)
    {
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderBy),
            [typeof(TEntity), keySelector.ReturnType],
            query.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(orderByCall);
    }

    /// <summary>
    /// AOT-safe OrderByDescending using Expression.Call instead of MakeGenericMethod.
    /// </summary>
    private static IOrderedQueryable<TEntity> ApplyOrderByDescendingAot(IQueryable<TEntity> query, LambdaExpression keySelector)
    {
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderByDescending),
            [typeof(TEntity), keySelector.ReturnType],
            query.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(orderByCall);
    }

    /// <summary>
    /// AOT-safe ThenBy using Expression.Call instead of MakeGenericMethod.
    /// </summary>
    private static IOrderedQueryable<TEntity> ApplyThenByAot(IOrderedQueryable<TEntity> query, LambdaExpression keySelector)
    {
        var thenByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.ThenBy),
            [typeof(TEntity), keySelector.ReturnType],
            query.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(thenByCall);
    }

    /// <summary>
    /// AOT-safe ThenByDescending using Expression.Call instead of MakeGenericMethod.
    /// </summary>
    private static IOrderedQueryable<TEntity> ApplyThenByDescendingAot(IOrderedQueryable<TEntity> query, LambdaExpression keySelector)
    {
        var thenByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.ThenByDescending),
            [typeof(TEntity), keySelector.ReturnType],
            query.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(thenByCall);
    }

    /// <summary>
    /// Applies extensions (lazy loading from external services) using AOT-safe approach.
    /// </summary>
    private async Task<TEntity> ApplyExtensionsAsync(TEntity entity, CancellationToken cancellationToken)
    {
        foreach (var extension in State.Extensions)
        {
            await ApplyExtensionAsync(entity, extension, cancellationToken);
        }
        return entity;
    }

    private async Task<List<TEntity>> ApplyExtensionsAsync(List<TEntity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            await ApplyExtensionsAsync(entity, cancellationToken);
        }
        return entities;
    }

    private async Task ApplyExtensionAsync(
        TEntity entity,
        Expression<Func<TEntity, object?>> extensionExpression,
        CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine($"[ApplyExtensionAsync] Starting for {typeof(TEntity).Name}");

        // Extract property info from expression
        if (extensionExpression.Body is not MemberExpression memberExpr)
        {
            if (extensionExpression.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression innerMemberExpr)
            {
                memberExpr = innerMemberExpr;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ApplyExtensionAsync] Expression body is not MemberExpression - returning");
                return;
            }
        }

        // AOT-safe: Use generated accessor instead of GetProperty reflection
        var accessor = EntityAccessorRegistry.GetAccessor<TEntity>();
        var propertyName = memberExpr.Member.Name;
        System.Diagnostics.Debug.WriteLine($"[ApplyExtensionAsync] PropertyName: {propertyName}, Accessor: {(accessor != null ? "Found" : "NULL - using reflection fallback")}");

        // Try generated accessor first, fallback to reflection if not available
        System.Reflection.PropertyInfo? propertyInfo;
        if (accessor is not null)
        {
            propertyInfo = accessor.GetPropertyInfo(propertyName);
        }
        else
        {
            // Fallback: when source generator is not available (e.g. project references)
            propertyInfo = typeof(TEntity).GetProperty(propertyName);
        }

        if (propertyInfo is null)
        {
            System.Diagnostics.Debug.WriteLine($"[ApplyExtensionAsync] PropertyInfo is null for {propertyName} - returning");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[ApplyExtensionAsync] PropertyInfo found: {propertyInfo.PropertyType.Name}");

        // Get current value - use accessor or reflection fallback
        object? currentValue;
        if (accessor is not null)
            currentValue = accessor.GetValue(entity, propertyName);
        else
            currentValue = propertyInfo.GetValue(entity);

        if (currentValue is not null)
            return;

        // Get foreign key value
        var foreignKeyName = propertyName + "Id";
        object? foreignKeyValue;
        if (accessor is not null)
        {
            foreignKeyValue = accessor.GetValue(entity, foreignKeyName);
        }
        else
        {
            var fkProperty = typeof(TEntity).GetProperty(foreignKeyName);
            foreignKeyValue = fkProperty?.GetValue(entity);
        }

        // Handle both Guid and Guid? (nullable) foreign keys
        // Note: boxed Guid? with value becomes Guid, so 'is Guid' works for both
        if (foreignKeyValue is not Guid idValue || idValue == Guid.Empty)
            return;

        // AOT-safe: Resolve extension service via non-generic interface
        var extensionKey = $"Extension:{propertyInfo.PropertyType.FullName}";
        await using var scope = ServiceProvider.CreateAsyncScope();

        // Try keyed service first (preferred for AOT)
        var extensionService = scope.ServiceProvider.GetKeyedService<IDatabaseExtension>(extensionKey);

        if (extensionService is null)
        {
            // AOT-safe: Use generated ExtensionTypeResolver instead of GetInterfaces
            var allExtensions = scope.ServiceProvider.GetServices<IDatabaseExtension>();
            extensionService = ExtensionTypeResolver.FindExtension(allExtensions, propertyInfo.PropertyType);
        }

        if (extensionService is null)
            return;

        // Call via non-generic interface (AOT-safe)
        var loadedValue = await extensionService.InitializeAsync(idValue, cancellationToken);

        if (loadedValue is not null)
        {
            // Set value using accessor or reflection fallback
            if (accessor is not null)
                accessor.SetValue(entity, propertyName, loadedValue);
            else
                propertyInfo.SetValue(entity, loadedValue);
        }
    }

    // === IExecutableQuery Implementation ===

    public override async Task<TEntity?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await GetFirstAsync(cancellationToken);
    }

    public override async Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default)
        where TDto : default
    {
        var entity = await GetAsync(cancellationToken);
        return MappingService.Map<TDto>(entity);
    }

    public override async Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        var query = await BuildQueryAsync(cancellationToken);
        var result = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (result is not null && State.Extensions.Count > 0)
        {
            result = await ApplyExtensionsAsync(result, cancellationToken);
        }

        return result;
    }

    public override async Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default)
        where TDto : default
    {
        var entity = await GetFirstAsync(cancellationToken);
        return MappingService.Map<TDto>(entity);
    }

    public override async Task<IReadOnlyList<TEntity>?> GetListAsync(CancellationToken cancellationToken = default)
    {
        var query = await BuildQueryAsync(cancellationToken);
        var result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (result.Count == 0)
            return null;

        if (State.Extensions.Count > 0)
        {
            result = await ApplyExtensionsAsync(result, cancellationToken);
        }

        return result;
    }

    public override async Task<IReadOnlyList<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default)
        where TDto : default
    {
        var entities = await GetListAsync(cancellationToken);
        return MappingService.MapList<TDto>(entities);
    }

    public override async Task<IReadOnlyList<TResult>?> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildQueryAsync(cancellationToken);
        var result = await query.Select(selector).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result.Count == 0 ? null : result;
    }

    public override async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        var query = await BuildQueryAsync(cancellationToken);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = await BuildQueryAsync(cancellationToken);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.Set<TEntity>().AsSplitQuery();

        if (State.Filter is not null)
        {
            query = query.Where(State.Filter);
        }

        var entities = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entities)
        {
            updateAction(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        var count = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return count > 0 ? count : null;
    }

    public override async Task<int> DeleteRangeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var query = context.Set<TEntity>().AsQueryable();

        if (State.Filter is not null)
        {
            query = query.Where(State.Filter);
        }

        // Use EF Core 7+ ExecuteDeleteAsync for bulk delete
        return await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
