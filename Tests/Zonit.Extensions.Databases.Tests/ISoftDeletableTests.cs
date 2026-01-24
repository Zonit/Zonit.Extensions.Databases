using Zonit.Extensions.Databases;

namespace Zonit.Extensions.Databases.Tests;

/// <summary>
/// Tests for ISoftDeletable interface.
/// </summary>
public class ISoftDeletableTests
{
    /// <summary>
    /// Entity with default OnSoftDelete implementation (does nothing).
    /// </summary>
    private class DefaultSoftDeletableEntity : ISoftDeletable
    {
        public DateTimeOffset? DeletedAt { get; set; }
    }

    /// <summary>
    /// Entity with custom OnSoftDelete implementation.
    /// </summary>
    private class CustomSoftDeletableEntity : ISoftDeletable
    {
        public DateTimeOffset? DeletedAt { get; set; }
        public bool WasCalled { get; private set; }
        public string State { get; private set; } = "Initial";

        public void OnSoftDelete()
        {
            WasCalled = true;
            State = "SoftDeleted";
        }
    }

    [Fact]
    public void DefaultOnSoftDelete_DoesNotThrow()
    {
        // Arrange
        ISoftDeletable entity = new DefaultSoftDeletableEntity();

        // Act & Assert - should not throw (default interface method)
        entity.OnSoftDelete();
    }

    [Fact]
    public void CustomOnSoftDelete_ExecutesCustomLogic()
    {
        // Arrange
        var entity = new CustomSoftDeletableEntity();
        Assert.False(entity.WasCalled);
        Assert.Equal("Initial", entity.State);

        // Act
        entity.OnSoftDelete();

        // Assert
        Assert.True(entity.WasCalled);
        Assert.Equal("SoftDeleted", entity.State);
    }

    [Fact]
    public void DeletedAt_InitiallyNull()
    {
        // Arrange & Act
        var entity = new DefaultSoftDeletableEntity();

        // Assert
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void DeletedAt_CanBeSet()
    {
        // Arrange
        var entity = new DefaultSoftDeletableEntity();
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        entity.DeletedAt = timestamp;

        // Assert
        Assert.Equal(timestamp, entity.DeletedAt);
    }

    [Fact]
    public void DeletedAt_CanBeCleared()
    {
        // Arrange
        var entity = new DefaultSoftDeletableEntity { DeletedAt = DateTimeOffset.UtcNow };
        Assert.NotNull(entity.DeletedAt);

        // Act
        entity.DeletedAt = null;

        // Assert
        Assert.Null(entity.DeletedAt);
    }
}
