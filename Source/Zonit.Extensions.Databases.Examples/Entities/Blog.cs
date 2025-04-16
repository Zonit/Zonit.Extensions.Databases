using System.ComponentModel.DataAnnotations.Schema;

namespace Zonit.Extensions.Databases.Examples.Entities;

public class Blog
{
    public Guid Id { get; set; }

    [NotMapped]
    public UserModel? User { get; set; }
    public Guid? UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; private set; } = DateTime.UtcNow;
}