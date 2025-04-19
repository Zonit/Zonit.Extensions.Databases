using Microsoft.Extensions.DependencyInjection;

namespace Zonit.Extensions.Databases.Examples;

public interface ICos<TEntity, TIdType>
{
    public Task<TEntity> GetAsync(TIdType id);
}

public class Organization // Model organizacji w abstrakcji
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IOrganizationEntity // Interfejs dla encji z organizacją
{
    public Organization? Organization { get; }
    public Guid? OrganizationId { get; set;  }
}

public class Article : IOrganizationEntity // Moduł użytkownika, oprócz abstrakcji to on nie ma istnienia o Organizacjach
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Organization? Organization { get; private set; }
    public Guid? OrganizationId { get; set; }
}

public class OrganizationService : ICos<Organization, Guid> // Obsługa eventu/zapytania itp
{
    public async Task<Organization> GetAsync(Guid id)
    {
        var model = new Organization
        {
            Id = id,
            Name = "Organization"
        };

        return model;
    }
}

public static class RegisterExtension   // Rejestracja obsługi wywołania
{
    public static IServiceCollection AddExtension<T, T2>(this IServiceCollection services)
    {
        services.AddTransient<ICos<Organization, Guid> , OrganizationService>();
        return services;
    }
}

public class Program2
{
    public static void Main()
    {
        var service = new ServiceCollection();
        service.AddExtension<IOrganizationEntity, Guid>();
    }
}

public class TestClass
{
    public void Test()
    {
        var context = new Article(); // DB Context, klasa która wykrywa intefejs w encji i obsługuje RegisterExtension

        if(context.Organization is not null)
            Console.WriteLine($"{context.Name} jest w {context.Organization.Name}");

    }
}