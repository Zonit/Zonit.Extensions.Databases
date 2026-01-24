using Zonit.Extensions.Databases.Tests.Entities;
using Zonit.Extensions.Databases.Tests.Infrastructure;

namespace Zonit.Extensions.Databases.Tests;

/// <summary>
/// Tests for DeleteAsync with soft delete functionality.
/// </summary>
public class DeleteAsyncTests
{
    #region Entity Without ISoftDeletable

    [Fact]
    public async Task DeleteAsync_EntityWithoutISoftDeletable_PerformsHardDelete()
    {
        // Arrange
        var factory = TestDbContextFactory.Create();
        var repository = new TestEntityRepository(factory);
        var entity = new TestEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity);

        // Assert
        Assert.True(result);
        var deleted = await repository.GetByIdAsync(entity.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_EntityWithoutISoftDeletable_WithForceDelete_PerformsHardDelete()
    {
        // Arrange
        var factory = TestDbContextFactory.Create();
        var repository = new TestEntityRepository(factory);
        var entity = new TestEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity, forceDelete: true);

        // Assert
        Assert.True(result);
        var deleted = await repository.GetByIdAsync(entity.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region Entity With ISoftDeletable - Default Behavior (Soft Delete)

    [Fact]
    public async Task DeleteAsync_SoftDeletableEntity_PerformsSoftDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity);

        // Assert
        Assert.True(result);

        // Entity should still exist in database with DeletedAt set
        await using var context = await factory.CreateDbContextAsync();
        var softDeleted = await context.SoftDeletableEntities.FindAsync(entity.Id);
        Assert.NotNull(softDeleted);
        Assert.NotNull(softDeleted!.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletableEntity_SetsDeletedAtTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        var beforeDelete = DateTimeOffset.UtcNow;

        // Act
        await repository.DeleteAsync(entity);

        var afterDelete = DateTimeOffset.UtcNow;

        // Assert
        await using var context = await factory.CreateDbContextAsync();
        var softDeleted = await context.SoftDeletableEntities.FindAsync(entity.Id);

        Assert.NotNull(softDeleted!.DeletedAt);
        Assert.True(softDeleted.DeletedAt >= beforeDelete);
        Assert.True(softDeleted.DeletedAt <= afterDelete);
    }

    #endregion

    #region Entity With ISoftDeletable - Force Delete (Hard Delete)

    [Fact]
    public async Task DeleteAsync_SoftDeletableEntity_WithForceDelete_PerformsHardDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity, forceDelete: true);

        // Assert
        Assert.True(result);

        // Entity should be completely removed from database
        await using var context = await factory.CreateDbContextAsync();
        var deleted = await context.SoftDeletableEntities.FindAsync(entity.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ById_SoftDeletableEntity_WithForceDelete_PerformsHardDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity.Id, forceDelete: true);

        // Assert
        Assert.True(result);

        // Entity should be completely removed from database
        await using var context = await factory.CreateDbContextAsync();
        var deleted = await context.SoftDeletableEntities.FindAsync(entity.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region Custom OnSoftDelete Implementation

    [Fact]
    public async Task DeleteAsync_CustomSoftDeletable_CallsOnSoftDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new CustomSoftDeletableEntityRepository(factory);
        var entity = new CustomSoftDeletableEntity { Name = "Test", Status = "Active" };
        await repository.AddAsync(entity);

        Assert.False(entity.OnSoftDeleteCalled);

        // Act
        await repository.DeleteAsync(entity);

        // Assert
        Assert.True(entity.OnSoftDeleteCalled);
        Assert.Equal("Deleted", entity.Status);
    }

    [Fact]
    public async Task DeleteAsync_CustomSoftDeletable_ChangesStatusInDatabase()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new CustomSoftDeletableEntityRepository(factory);
        var entity = new CustomSoftDeletableEntity { Name = "Test", Status = "Active" };
        await repository.AddAsync(entity);

        // Act
        await repository.DeleteAsync(entity);

        // Assert
        await using var context = await factory.CreateDbContextAsync();
        var softDeleted = await context.CustomSoftDeletableEntities.FindAsync(entity.Id);

        Assert.NotNull(softDeleted);
        Assert.Equal("Deleted", softDeleted!.Status);
        Assert.NotNull(softDeleted.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_CustomSoftDeletable_WithForceDelete_DoesNotCallOnSoftDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new CustomSoftDeletableEntityRepository(factory);
        var entity = new CustomSoftDeletableEntity { Name = "Test", Status = "Active" };
        await repository.AddAsync(entity);

        // Act
        await repository.DeleteAsync(entity, forceDelete: true);

        // Assert
        Assert.False(entity.OnSoftDeleteCalled);
        Assert.Equal("Active", entity.Status); // Status should not change
    }

    #endregion

    #region Delete By Id

    [Fact]
    public async Task DeleteAsync_ById_SoftDeletableEntity_PerformsSoftDelete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.DeleteAsync(entity.Id);

        // Assert
        Assert.True(result);

        // Entity should still exist in database with DeletedAt set
        await using var context = await factory.CreateDbContextAsync();
        var softDeleted = await context.SoftDeletableEntities.FindAsync(entity.Id);
        Assert.NotNull(softDeleted);
        Assert.NotNull(softDeleted!.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ById_NotFound_ReturnsFalse()
    {
        // Arrange
        var factory = TestDbContextFactory.Create();
        var repository = new TestEntityRepository(factory);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Restore Tests

    [Fact]
    public async Task RestoreAsync_SoftDeletedEntity_RestoresEntity()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);
        await repository.DeleteAsync(entity); // Soft delete

        // Act
        var result = await repository.RestoreAsync(entity.Id);

        // Assert
        Assert.True(result);

        await using var context = await factory.CreateDbContextAsync();
        var restored = await context.SoftDeletableEntities.FindAsync(entity.Id);
        Assert.NotNull(restored);
        Assert.Null(restored!.DeletedAt);
    }

    [Fact]
    public async Task RestoreAsync_NotSoftDeleted_ReturnsFalse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = TestDbContextFactory.Create(dbName);
        var repository = new SoftDeletableEntityRepository(factory);
        var entity = new SoftDeletableEntity { Name = "Test" };
        await repository.AddAsync(entity);

        // Act - try to restore non-deleted entity
        var result = await repository.RestoreAsync(entity.Id);

        // Assert
        Assert.False(result);
    }

    #endregion
}
