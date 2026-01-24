using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer;

namespace Zonit.Extensions.Databases.Examples.Repositories;

/// <summary>
/// Blog repository implementation with simplified constructor.
/// </summary>
/// <remarks>
/// <para>
/// Uses <see cref="RepositoryContext{TContext}"/> for simplified dependency injection.
/// All required dependencies (IDbContextFactory, IServiceProvider) are bundled in one container.
/// </para>
/// <para>
/// <b>Available context options:</b>
/// <list type="bullet">
/// <item><see cref="SqlServerRepository{TEntity, TContext}.CreateContextAsync"/> - creates new DbContext for each operation (recommended for parallel queries)</item>
/// <item>Protected <c>ContextFactory</c> property - for advanced scenarios requiring multiple contexts</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple repository - one parameter!
/// internal class BlogRepository(RepositoryContext&lt;DatabaseContext&gt; context)
///     : SqlServerRepository&lt;Blog, DatabaseContext&gt;(context), IBlogRepository;
///
/// // With custom dependencies
/// internal class BlogRepository(
///     RepositoryContext&lt;DatabaseContext&gt; context,
///     IMyCustomService myService)
///     : SqlServerRepository&lt;Blog, DatabaseContext&gt;(context), IBlogRepository
/// {
///     public async Task CustomMethodAsync()
///     {
///         // Use myService and CreateContextAsync()
///     }
/// }
/// </code>
/// </example>
internal class BlogRepository(RepositoryContext<DatabaseContext> context)
    : SqlServerRepository<Blog, DatabaseContext>(context), IBlogRepository
{
    /// <summary>
    /// Custom method demonstrating DbContext usage via CreateContextAsync.
    /// </summary>
    public async Task GetCustomAsync()
    {
        await using var dbContext = await CreateContextAsync();
        var blog = await dbContext.Blogs
            .FirstOrDefaultAsync(b => b.Created > DateTime.UtcNow.AddDays(-30));

        if (blog is null)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  Nie znaleziono żadnych wpisów bloga   ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            return;
        }

        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║            Znaleziono blog             ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine($"║  ID: {blog.Id}");
        Console.WriteLine($"║  Tytuł: {blog.Title}");
        Console.WriteLine($"║  Autor: {blog.User?.Name ?? "Nieznany"}");
        Console.WriteLine($"║  Data utworzenia: {blog.Created:yyyy-MM-dd HH:mm}");
        Console.WriteLine("╚════════════════════════════════════════╝");
    }
}