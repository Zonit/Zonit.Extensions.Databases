namespace Zonit.Extensions.Databases.Examples.Entities;

/// <summary>
/// Blog entity with extension property for User.
/// Note: No [NotMapped] attribute needed - use modelBuilder.IgnoreExtensionProperties() in OnModelCreating.
/// </summary>
public class Blog
{
    public Guid Id { get; set; }

    /// <summary>
    /// User extension - loaded via IDatabaseExtension, not EF Core.
    /// Automatically ignored by IgnoreExtensionProperties() convention.
    /// </summary>
    public UserModel? User { get; set; }
    public Guid? UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; private set; } = DateTime.UtcNow;
}