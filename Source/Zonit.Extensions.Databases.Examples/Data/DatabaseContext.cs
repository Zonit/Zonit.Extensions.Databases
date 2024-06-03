using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Data;

/*
 * Migration command: dotnet ef migrations add Examples_v1 
 */
internal class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var name = modelBuilder.Entity(entity.Name).Metadata.ClrType.Name;
            modelBuilder.Entity(entity.Name).ToTable($"Examples.{name}", "Zonit");
        };

        base.OnModelCreating(modelBuilder);
    }
}