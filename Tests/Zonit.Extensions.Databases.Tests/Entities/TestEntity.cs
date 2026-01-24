using Zonit.Extensions.Databases;

namespace Zonit.Extensions.Databases.Tests.Entities;

/// <summary>
/// Test entity without soft delete support.
/// </summary>
public class TestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Test entity with soft delete support.
/// </summary>
public class SoftDeletableEntity : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }
}

/// <summary>
/// Test entity with soft delete and custom OnSoftDelete implementation.
/// </summary>
public class CustomSoftDeletableEntity : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset? DeletedAt { get; set; }
    public bool OnSoftDeleteCalled { get; set; }

    /// <summary>
    /// Custom soft delete logic - sets status and marks that method was called.
    /// </summary>
    public void OnSoftDelete()
    {
        Status = "Deleted";
        OnSoftDeleteCalled = true;
    }
}
