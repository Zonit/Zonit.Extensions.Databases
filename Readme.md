# Zonit.Extensions.Databases

Zonit.Extensions.Databases is a flexible library for building repositories and handling CRUD operations on databases.  
It provides abstractions and interfaces, making it easy to manage database access and to extend your repositories with custom logic, REST API data, or other external sources.  
You can use your own repositories and expand them anytime with additional functions, while still keeping your codebase clean and modular.

## :package: NuGet Packages

### Abstraction
```
Install-Package Zonit.Extensions.Databases.Abstractions 
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Databases.Abstractions.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Databases.Abstractions.svg)

### SQL Server Implementation
```
Install-Package Zonit.Extensions.Databases.SqlServer
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Databases.SqlServer.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Databases.SqlServer.svg)


## :rocket: Features

- **Simplified Repository** - Single dependency constructor with `RepositoryContext<TContext>`
- **Fluent Query API** - `Where`, `Include`, `ThenInclude`, `OrderBy`, `Skip`, `Take` and more
- **Direct API on Repository** - No `AsQuery()` needed, just `repo.Where(...).GetListAsync()`
- **Table Naming Convention** - `SetPrefix("Schema", "Prefix")` for automatic table configuration
- **Extension Properties Convention** - Automatic in `DatabaseContextBase` (no manual call needed)
- **Protected Context Access** - `CreateContextAsync()` and `ContextFactory` for custom queries
- **Smart Delete** - Single `DeleteAsync()` method with automatic soft delete detection
- **Value Objects Support** - Automatic EF Core converters for `Title`, `Price`, `Culture`, etc.


## :star2: Quick Start

### 1. Create Repository (Simplified!)

```csharp
// Single RepositoryContext<TContext> contains everything you need
internal class BlogRepository(RepositoryContext<DatabaseContext> context)
    : SqlServerRepository<Blog, DatabaseContext>(context), IBlogRepository
{
    // Custom method with direct database access
    public async Task<Blog?> GetRecentAsync()
    {
        // Protected method for direct DbContext access
        await using var context = await CreateContextAsync();
        
        return await context.Blogs
            .Where(b => b.Created > DateTime.UtcNow.AddDays(-7))
            .FirstOrDefaultAsync();
    }
}
```

### 2. Entity Model (No [NotMapped] needed!)

```csharp
public class Blog
{
    public Guid Id { get; set; }

    // No [NotMapped] attribute needed - use IgnoreExtensionProperties() in OnModelCreating
    public UserModel? User { get; set; }
    public Guid? UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; private set; } = DateTime.UtcNow;
}
```

### 3. DbContext Configuration (Simplified!)

```csharp
internal class DatabaseContext(DbContextOptions<DatabaseContext> options) 
    : DatabaseContextBase<DatabaseContext>(options)
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simplified table naming - replaces manual foreach loop
        // Creates tables: [Zonit].[Examples.Blog], [Zonit].[Examples.User]
        modelBuilder.SetPrefix("Zonit", "Examples");

        // Call base to enable:
        // - Value Objects converters
        // - Auto-ignore extension properties (configurable via AutoIgnoreExtensionProperties)
        base.OnModelCreating(modelBuilder);
    }
    
    // Optional: Disable auto-ignoring extension properties
    // protected override bool AutoIgnoreExtensionProperties => false;
}
```


## :book: Fluent Query API

All query methods are available directly on the repository - no `AsQuery()` call needed!

### Filter with Where

```csharp
var blogs = await repo.Where(x => x.Title.Contains("C#")).GetListAsync();
var firstBlog = await repo.Where(x => x.Title == "Hello").GetFirstAsync();
```

### Eager Loading with Include / ThenInclude

```csharp
// Single navigation property
var blog = await repo
    .Include(x => x.User)
    .Where(x => x.Id == blogId)
    .GetAsync();

// ThenInclude for nested navigation
var blog = await repo
    .Include(x => x.User)
        .ThenInclude(u => u.Organization)
    .Where(x => x.Id == blogId)
    .GetAsync();

// Collection includes (same method, different signature)
var blog = await repo
    .Include(x => x.Comments)
        .ThenInclude(c => c.Author)
    .GetAsync();
```

### Ordering

```csharp
var blogs = await repo
    .OrderByDescending(x => x.Created)
    .ThenBy(x => x.Title)
    .GetListAsync();
```

### Pagination

```csharp
var page = await repo
    .OrderBy(x => x.Created)
    .Skip(20)
    .Take(10)
    .GetListAsync();
```

### Count and Exists

```csharp
var count = await repo.CountAsync();
var exists = await repo.Where(x => x.Title == "Test").AnyAsync();
```


## :gear: CRUD Operations

### Create

```csharp
var blog = await repo.AddAsync(new Blog
{
    Title = "Hello World",
    Content = "Example content"
});
```

### Read

```csharp
// By ID
var blog = await repo.GetByIdAsync(blogId);

// With DTO mapping
var blogDto = await repo.GetByIdAsync<BlogDto>(blogId);

// First with filter
var blog = await repo.Where(x => x.Title == "Hello").GetFirstAsync();

// All
var blogs = await repo.GetListAsync();
```

### Update

```csharp
// By ID with action
await repo.UpdateAsync(blogId, entity => entity.Title = "New Title");

// Or pass entity
blog.Title = "New Title";
await repo.UpdateAsync(blog);
```

### Delete

```csharp
// Default: soft delete if entity implements ISoftDeletable, otherwise hard delete
await repo.DeleteAsync(blogId);
// or
await repo.DeleteAsync(blog);

// Force hard delete (permanently remove even if ISoftDeletable)
await repo.DeleteAsync(blogId, forceDelete: true);
```

### Soft Delete with ISoftDeletable

Implement `ISoftDeletable` interface for automatic soft delete behavior:

```csharp
// Entity implements interface with optional custom logic
public class Blog : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; } = "active";
    
    // Required by ISoftDeletable
    public DateTimeOffset? DeletedAt { get; set; }
    
    // Optional: Override to add custom logic when soft deleting
    public void OnSoftDelete()
    {
        Status = "deleted";
        // Add any additional cleanup logic here
    }
}

// Usage - automatically sets DeletedAt and calls OnSoftDelete()
await repo.DeleteAsync(blogId);

// Restore (clears DeletedAt)
await repo.RestoreAsync(blogId);
```

**Delete behavior:**
| Entity Type | `forceDelete: false` (default) | `forceDelete: true` |
|-------------|-------------------------------|---------------------|
| Implements `ISoftDeletable` | Soft delete (sets `DeletedAt`, calls `OnSoftDelete()`) | Hard delete (removes from DB) |
| Does NOT implement `ISoftDeletable` | Hard delete | Hard delete |


## :bulb: Extension Properties

Load data from external sources (API, other services) into your entities:

### 1. Create Extension Handler

```csharp
public class UserExtension : IDatabaseExtension<UserModel>
{
    public async Task<UserModel> InitializeAsync(Guid userId, CancellationToken ct = default)
    {
        // Fetch from external API, cache, or compute
        return new UserModel { Id = userId, Name = "Loaded from API" };
    }
}
```

### 2. Use in Query

```csharp
var blog = await repo
    .Extension(x => x.User)  // Load User via extension
    .Where(x => x.Id == blogId)
    .GetAsync();

Console.WriteLine(blog.User?.Name); // "Loaded from API"
```


## :package: Value Objects Support

Inherit from `DatabaseContextBase<T>` to automatically enable EF Core converters:

| Value Object | Database Type | Max Length | Use Case |
|-------------|---------------|------------|----------|
| `Culture` | `NVARCHAR(10)` | 10 | Language codes (en-US, pl-PL) |
| `UrlSlug` | `NVARCHAR(200)` | 200 | SEO-friendly URLs |
| `Title` | `NVARCHAR(60)` | 60 | Page/content titles |
| `Description` | `NVARCHAR(160)` | 160 | Meta descriptions |
| `Content` | `NVARCHAR(MAX)` | - | Large text content |
| `Url` | `NVARCHAR(2048)` | 2048 | URLs with validation |
| `Price` | `DECIMAL(19,8)` | - | Product prices (non-negative) |
| `Money` | `DECIMAL(19,8)` | - | Balances, transactions (can be negative) |
| `Color` | `NVARCHAR(100)` | 100 | Colors in OKLCH format |
| `Asset` | `VARBINARY(MAX)` | - | Files with metadata |

```csharp
public class Article
{
    public Guid Id { get; set; }
    public Title Title { get; set; }           // Auto-converted, max 60 chars
    public Description Description { get; set; } // Auto-converted, max 160 chars
    public Price Price { get; set; }            // Auto-converted, decimal(19,8)
}
```


## :wrench: Service Registration

```csharp
// Register DbContext factory
builder.Services.AddDbSqlServer<DatabaseContext>(options => 
    options.UseSqlServer(connectionString));

// Register repository - IServiceProvider is optional now!
builder.Services.AddTransient<IBlogRepository, BlogRepository>();
```


## :information_source: For more examples
See the `Example` project included in the repository.
