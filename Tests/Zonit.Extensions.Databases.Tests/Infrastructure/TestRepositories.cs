using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.SqlServer;
using Zonit.Extensions.Databases.Tests.Entities;

namespace Zonit.Extensions.Databases.Tests.Infrastructure;

/// <summary>
/// Test repository for TestEntity.
/// </summary>
public class TestEntityRepository : SqlServerRepository<TestEntity, TestDbContext>
{
    public TestEntityRepository(
        IDbContextFactory<TestDbContext> contextFactory,
        IServiceProvider? serviceProvider = null)
        : base(contextFactory, serviceProvider!)
    {
    }
}

/// <summary>
/// Test repository for SoftDeletableEntity.
/// </summary>
public class SoftDeletableEntityRepository : SqlServerRepository<SoftDeletableEntity, TestDbContext>
{
    public SoftDeletableEntityRepository(
        IDbContextFactory<TestDbContext> contextFactory,
        IServiceProvider? serviceProvider = null)
        : base(contextFactory, serviceProvider!)
    {
    }
}

/// <summary>
/// Test repository for CustomSoftDeletableEntity.
/// </summary>
public class CustomSoftDeletableEntityRepository : SqlServerRepository<CustomSoftDeletableEntity, TestDbContext>
{
    public CustomSoftDeletableEntityRepository(
        IDbContextFactory<TestDbContext> contextFactory,
        IServiceProvider? serviceProvider = null)
        : base(contextFactory, serviceProvider!)
    {
    }
}
