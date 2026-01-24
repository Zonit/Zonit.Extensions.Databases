using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer;
using Zonit.Extensions.Databases.SqlServer.Extensions;

namespace Zonit.Extensions.Databases.Examples.Data;

/*
 * Migration command: dotnet ef migrations add Examples_v1 
 */
internal class DatabaseContext(DbContextOptions<DatabaseContext> options) : DatabaseContextBase<DatabaseContext>(options)
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Simplified table naming (replaces manual foreach loop)
        modelBuilder.SetPrefix("Zonit", "Examples");

        // ✅ Call base - automatically:
        // - Ignores extension properties (no [NotMapped] needed)
        // - Configures Zonit Value Objects
        base.OnModelCreating(modelBuilder);
    }
}