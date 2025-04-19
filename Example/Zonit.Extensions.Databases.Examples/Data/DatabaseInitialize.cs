using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Data;

/// <summary>
/// Initialization of the database, checking whether there is a migration, whether there is basic data in the database.
/// </summary>
/// <param name="context"></param>
internal class DatabaseInitialize(IDbContextFactory<DatabaseContext> context) : IHostedService
{
    readonly DatabaseContext _context = context.CreateDbContext();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.ExampleDataBlogs();
    }

    async Task ExampleDataBlogs()
    {
        if (await _context.Blogs.AnyAsync())
            return;

        var currencies = new List<Blog>
        {
            new() { 
                Id = Guid.Parse("f74410c7-6972-4813-8b01-08dc5f359b7e"), 
                Title = "Hello World", 
                Content = "Content..." 
            },
        };

        _context.Blogs.AddRange(currencies);
        await _context.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}