namespace Zonit.Extensions.Databases;

/// <summary>
/// Interface for entities that support soft delete.
/// Implement this interface to enable automatic soft delete behavior when calling DeleteAsync.
/// </summary>
/// <remarks>
/// <para>
/// When <c>DeleteAsync(entity)</c> or <c>DeleteAsync(id)</c> is called without <c>forceDelete = true</c>,
/// the repository will automatically perform a soft delete by setting <see cref="DeletedAt"/> and calling <see cref="OnSoftDelete"/>.
/// </para>
/// <para>
/// Override <see cref="OnSoftDelete"/> to add custom logic (e.g., change status, clear relationships).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Blog : ISoftDeletable
/// {
///     public Guid Id { get; set; }
///     public string Title { get; set; }
///     public string Status { get; set; } = "active";
///     public DateTimeOffset? DeletedAt { get; set; }
///     
///     public void OnSoftDelete()
///     {
///         Status = "deleted";
///     }
/// }
/// 
/// // Usage:
/// await repo.DeleteAsync(blog);           // Soft delete (sets DeletedAt + calls OnSoftDelete)
/// await repo.DeleteAsync(blog, true);     // Hard delete (permanent removal)
/// </code>
/// </example>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// Null means the entity is not deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Called during soft delete to perform additional cleanup or state changes.
    /// Override this method to add custom logic like changing status, clearing relationships, etc.
    /// </summary>
    /// <remarks>
    /// This method is called after <see cref="DeletedAt"/> is set but before SaveChanges.
    /// Default implementation does nothing.
    /// </remarks>
    void OnSoftDelete() { }
}
