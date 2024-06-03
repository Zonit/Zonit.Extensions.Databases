namespace Zonit.Extensions.Databases.Examples.Entities;

public class Blog
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; private set; } = DateTime.UtcNow;
}