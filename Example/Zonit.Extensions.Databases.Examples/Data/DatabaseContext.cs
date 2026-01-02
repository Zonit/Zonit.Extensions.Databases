using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer;

namespace Zonit.Extensions.Databases.Examples.Data;

/*
 * Migration command: dotnet ef migrations add Examples_v1 
 */
internal class DatabaseContext(DbContextOptions<DatabaseContext> options) : ZonitDbContext<DatabaseContext>(options)
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Your custom table configuration
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var name = modelBuilder.Entity(entity.Name).Metadata.ClrType.Name;
            modelBuilder.Entity(entity.Name).ToTable($"Examples.{name}", "Zonit");
        }

        // ✅ Call base to automatically configure Zonit Value Objects
        base.OnModelCreating(modelBuilder);
    }
}