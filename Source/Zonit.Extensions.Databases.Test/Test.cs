namespace Zonit.Extensions.Databases.Test;

internal class Test
{
}

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IOrganizationEntity
{
    public Organization? Organization { get; set; }
    public Guid? OrganizationId { get; set; }
}


public class User : IOrganizationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Organization? Organization { get; set; }
    public Guid? OrganizationId { get; set; }
}

public class Program
{
    public static void Main()
    {
        var user = new User();
        user.Organization = new Organization();
        user.OrganizationId = Guid.NewGuid();
    }
}
