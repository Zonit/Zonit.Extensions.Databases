using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Tests.Entities;

namespace Zonit.Extensions.Databases.Tests.Infrastructure;

/// <summary>
/// Test database context for unit testing.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<SoftDeletableEntity> SoftDeletableEntities => Set<SoftDeletableEntity>();
    public DbSet<CustomSoftDeletableEntity> CustomSoftDeletableEntities => Set<CustomSoftDeletableEntity>();
}
