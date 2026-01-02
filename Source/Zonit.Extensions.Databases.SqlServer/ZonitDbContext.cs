using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.SqlServer.Extensions;

namespace Zonit.Extensions.Databases.SqlServer;

/// <summary>
/// Base DbContext with automatic configuration for Zonit Value Objects.
/// Inherit from this class to automatically enable Culture, UrlSlug, Title, Description, and Price value objects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ZonitDbContext"/> class.
/// </remarks>
/// <param name="options">The options for this context.</param>
public abstract class ZonitDbContext(DbContextOptions options) : DbContext(options)
{

    /// <summary>
    /// Configures the model with Zonit Value Objects converters.
    /// Override this method to add additional configuration, but always call base.OnModelCreating(modelBuilder).
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically configure Zonit Value Objects
        modelBuilder.UseZonitValueObjects();

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Generic base DbContext with automatic configuration for Zonit Value Objects.
/// Inherit from this class to automatically enable Culture, UrlSlug, Title, Description, and Price value objects.
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
public abstract class ZonitDbContext<TContext> : DbContext where TContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZonitDbContext{TContext}"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    protected ZonitDbContext(DbContextOptions<TContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model with Zonit Value Objects converters.
    /// Override this method to add additional configuration, but always call base.OnModelCreating(modelBuilder).
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically configure Zonit Value Objects
        modelBuilder.UseZonitValueObjects();

        base.OnModelCreating(modelBuilder);
    }
}
