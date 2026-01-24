namespace Zonit.Extensions.Databases;

/// <summary>
/// Marks a partial class as a repository for the specified entity and database context.
/// The source generator will automatically generate the constructor and context properties.
/// </summary>
/// <remarks>
/// <para>
/// Usage example:
/// </para>
/// <code>
/// [Repository&lt;Blog, DatabaseContext&gt;]
/// internal partial class BlogRepository : IBlogRepository
/// {
///     // Custom methods using generated properties:
///     
///     // Use DbContext for simple, scoped operations:
///     public async Task&lt;Blog?&gt; GetByIdAsync(Guid id)
///         => await DbContext.Blogs.FindAsync(id);
///     
///     // Use ContextFactory for parallel operations (pagination, bulk):
///     public async Task&lt;(List&lt;Blog&gt; Items, int Total)&gt; GetPagedAsync(int page, int size)
///     {
///         await using var ctx1 = await ContextFactory.CreateDbContextAsync();
///         var items = await ctx1.Blogs.Skip(page * size).Take(size).ToListAsync();
///         
///         await using var ctx2 = await ContextFactory.CreateDbContextAsync();
///         var total = await ctx2.Blogs.CountAsync();
///         
///         return (items, total);
///     }
/// }
/// </code>
/// <para>
/// The source generator creates:
/// </para>
/// <list type="bullet">
///     <item><description><c>DbContext</c> - Scoped database context for simple operations</description></item>
///     <item><description><c>ContextFactory</c> - Factory for creating new contexts (parallel operations)</description></item>
///     <item><description><c>ServiceProvider</c> - For resolving additional dependencies</description></item>
///     <item><description>Constructor with <c>IDbContextFactory&lt;TContext&gt;</c> and <c>IServiceProvider</c></description></item>
/// </list>
/// <para>
/// <b>When to use DbContext vs ContextFactory:</b>
/// </para>
/// <list type="table">
///     <listheader>
///         <term>Property</term>
///         <description>Use Case</description>
///     </listheader>
///     <item>
///         <term><c>DbContext</c></term>
///         <description>Simple CRUD, single queries, change tracking within scope</description>
///     </item>
///     <item>
///         <term><c>ContextFactory</c></term>
///         <description>Parallel queries, pagination (items + count), bulk operations, long-running tasks</description>
///     </item>
/// </list>
/// </remarks>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
/// <typeparam name="TContext">The database context type (must inherit from DbContext).</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RepositoryAttribute<TEntity, TContext> : Attribute
    where TEntity : class
    where TContext : class
{
}
