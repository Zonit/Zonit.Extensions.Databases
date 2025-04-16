namespace Zonit.Extensions.Databases.Examples.Entities;

public class UserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayRole { get; set; } = "User";
    public string? Avatar { get; set; }

    public string? FullName => $"{FirstName} {LastName}";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<string> Policy { get; set; } = [];
    public List<string> Roles { get; set; } = [];
}