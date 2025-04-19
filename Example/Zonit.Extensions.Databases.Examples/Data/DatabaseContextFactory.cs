using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Zonit.Extensions.Databases.Examples.Data;

/// <summary>
/// Only factory migration
/// </summary>
internal class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Zonit.Extensions.Databases.Examples;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true;");

        return new DatabaseContext(optionsBuilder.Options);
    }
}