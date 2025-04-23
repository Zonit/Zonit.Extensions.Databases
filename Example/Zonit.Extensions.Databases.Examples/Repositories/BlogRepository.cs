using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer;
using Zonit.Extensions.Databases.SqlServer.Repositories;

namespace Zonit.Extensions.Databases.Examples.Repositories;

internal class BlogRepository(Context<DatabaseContext> context) : DatabaseRepository<Blog>(context), IBlogRepository
{
    /// <summary>
    /// Przykład własnej implementacji metody dostępu do danych 
    /// z bezpośrednim wykorzystaniem kontekstu bazy danych.
    /// </summary>
    public async Task GetCustomAsync()
    {
        // Bezpośredni dostęp do kontekstu bazy danych
        var dbContext = context.DbContext;

        // Wykonanie zapytania za pomocą EF Core
        var blog = await dbContext.Blogs
            .FirstOrDefaultAsync(b => b.Created > DateTime.UtcNow.AddDays(-30));

        // Sprawdzenie wyników i własna logika biznesowa
        if (blog is null)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  Nie znaleziono żadnych wpisów bloga   ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            return;
        }

        // Formatowanie i wyświetlenie wyniku
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║            Znaleziono blog             ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine($"║  ID: {blog.Id}");
        Console.WriteLine($"║  Tytuł: {blog.Title}");
        Console.WriteLine($"║  Autor: {blog.User?.Name ?? "Nieznany"}");
        Console.WriteLine($"║  Data utworzenia: {blog.Created:yyyy-MM-dd HH:mm}");
        Console.WriteLine("╚════════════════════════════════════════╝");
    }
}