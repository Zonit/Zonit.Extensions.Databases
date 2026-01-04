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

- Speed up repository work — focus on your logic, not on database “plumbing”.
- Simple abstractions for common CRUD operations.
- Full support for DTO models.
- Built-in support for query building (`Where`, `Include`, `Select`, paging, ordering, and more).
- Plug-and-play: Easily extend repositories with custom methods or external data without touching the extension itself.


## :bulb: **New! Extension Methods for External Data**

You can easily **include properties and data from outside your database**, for example, by pulling from an external API or service.  
This is useful when your model should have extra fields, computed properties, or needs to include data fetched live (not loaded from your DB).

### How it works

1. **Create an Extension Class** implementing `IDatabaseExtension<TModel>`
    - In this class, implement business logic for fetching or computing the desired data.

```csharp
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Extensions;

public class UserExtension : IDatabaseExtension<UserModel>
{
    public async Task<UserModel> InitializeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Here you can call a REST API or any other data source.
        var model = new UserModel { 
            Id = userId,
            Name = "UserName",
        };
        return await Task.FromResult(model);
    }
}
```

2. **Reference the extension using `.Extension()`** in your repository query chain.

```csharp
var user = await _userRepository.Extension(x => x.UserExtension).GetByIdAsync(userId);
```

The `.Extension(x => x.UserExtension)` call tells the repository to supply or load the `UserExtension` property for your entity.  
This can be `virtual`, `NotMapped`, or simply a property populated on demand.

**Use case:**  
Suppose your `Blog` entity has a `UserModel? User` property, but you want to always fetch the latest user data from an API instead of the DB.  
Simply create an extension and reference it. The fetching, mapping, and attach process is handled by the extension system for you.


## :book: Example Usage

### Entity Model

```csharp
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
```

### DTO Example

```csharp
internal class BlogDto(Blog x)
{
    public string Id { get; set; } = $"Id: {x.Id}";
    public string Title { get; set; } = $"Title: {x.Title}";
    public string Content { get; set; } = $"Content: {x.Content}";
    public string Created { get; set; } = $"Created: {x.Created:G}";
}

public class UserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### Repository Implementation

```csharp
public interface IBlogRepository : IDatabaseRepository<Blog> { }

internal class BlogRepository(DatabaseContext _context)
    : DatabaseRepository<Blog>(_context), IBlogRepository { }
```

### Service Registration / Dependency Injection

```csharp
builder.Services.AddDbSqlServer<DatabaseContext>();

builder.Services.AddTransient<IBlogRepository, BlogRepository>();
```

> **Note:** Your `DatabaseContext` must inherit from `ZonitDbContext<T>` to automatically enable Value Objects support.  
> See [Value Objects Support](#value-objects-support) section below.

## :gear: CRUD and Query Operations

### Create

```csharp
var blog = await _blogRepository.AddAsync(new Blog
{
    Title = "Hello World",
    Content = "Example content"
});
```

### Read (Single or DTO)

```csharp
var blogSingle = await _blogRepository.GetByIdAsync(blogId);
var blogSingleDto = await _blogRepository.GetByIdAsync<BlogDto>(blogId);
```

### Query First (with conditions)

```csharp
var firstBlog = await _blogRepository.Where(x => x.Title == "Hello World").GetFirstAsync();
var firstBlogDto = await _blogRepository.Where(x => x.Title == "Hello World").GetFirstAsync<BlogDto>();
```

### Update

```csharp
var updated = await _blogRepository.UpdateAsync(blog.Id, entity =>
{
    entity.Title = "New Title";
});
```

or

```csharp
blog.Title = "New Title";
var updated = await _blogRepository.UpdateAsync(blog);
```

### Delete

```csharp
var deleted = await _blogRepository.DeleteAsync(blog.Id);
```
or
```csharp
var deleted = await _blogRepository.DeleteAsync(blog);
```

### Read All

```csharp
var blogs = await _blogRepository.GetListAsync();
var blogsDto = await _blogRepository.GetListAsync<BlogDto>();
```

## :page_with_curl: Repository APIs

Below is an **overview of the main interfaces and methods**.  
(See XML comments in code for details.)

---

### `IDatabaseRepository<TEntity>`

The main repository interface combines several specialized interfaces for database operations.

```csharp
// Queryable interface for building complex queries
IDatabaseAsQueryable<TEntity> AsQuery();

// Query building and filtering:
IDatabaseQueryOperations<TEntity> Extension(Expression<Func<TEntity, object?>> extensionExpression);
IDatabaseQueryOperations<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);
IDatabaseQueryOperations<TEntity> Include(Expression<Func<TEntity, object?>> includeExpression);
IDatabaseQueryOperations<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression);
IDatabaseQueryOperations<TEntity> WhereFullText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);
IDatabaseQueryOperations<TEntity> WhereFreeText(Expression<Func<TEntity, string>> propertySelector, string searchTerm);

// Pagination:
IDatabaseMultipleQueryable<TEntity> Skip(int count);
IDatabaseMultipleQueryable<TEntity> Take(int count);

// Sorting:
IDatabaseMultipleRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
IDatabaseMultipleRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);

// Single record access:
Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default);
Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<TDto?> GetByIdAsync<TDto>(Guid id, CancellationToken cancellationToken = default);
Task<TEntity?> GetAsync(CancellationToken cancellationToken = default);
Task<TDto?> GetAsync<TDto>(CancellationToken cancellationToken = default);

// Existence check:
Task<bool> AnyAsync(CancellationToken cancellationToken = default);

// Add operations:
Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);

// Update operations:
Task<bool> UpdateAsync(int id, Action<TEntity> updateAction, CancellationToken cancellationToken = default);
Task<bool> UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default);
Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

// Delete operations:
Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

// Multiple records access:
Task<IReadOnlyCollection<TEntity>?> GetListAsync(CancellationToken cancellationToken = default);
Task<IReadOnlyCollection<TDto>?> GetListAsync<TDto>(CancellationToken cancellationToken = default);

Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);
Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);
Task<int> GetCountAsync(CancellationToken cancellationToken = default);
```

---

**Note:**  
The deprecated `IDatabasesRepository<TEntity>` interface has been removed and is not supported anymore. Please migrate to `IDatabaseRepository<TEntity>` and related interfaces for all new development.

---

## :star2: Why use this extension?
- Build repository pattern code faster and cleaner.
- Decouple your database logic from your app logic.
- Easily extend your repositories with custom business logic or fetch data from external APIs with zero changes to the

---

## :package: Value Objects Support

This library provides **built-in support for Value Objects** from `Zonit.Extensions` package.  
Simply inherit your DbContext from `ZonitDbContext<T>` to automatically enable EF Core converters for:

| Value Object | Database Type | Max Length | Use Case |
|-------------|---------------|------------|----------|
| `Culture` | `NVARCHAR(10)` | 10 | Language codes (en-US, pl-PL) |
| `UrlSlug` | `NVARCHAR(200)` | 200 | SEO-friendly URLs |
| `Title` | `NVARCHAR(60)` | 60 | Page/content titles |
| `Description` | `NVARCHAR(160)` | 160 | Meta descriptions |
| `Price` | `DECIMAL(19,8)` | - | Monetary values |

### Quick Example

```csharp
using Zonit.Extensions;
using Zonit.Extensions.Databases.SqlServer;

// Entity with Value Objects
public class Article
{
    public Guid Id { get; set; }
    public Title Title { get; set; }
    public Description Description { get; set; }
    public UrlSlug Slug { get; set; }
    public Culture Culture { get; set; }
}

// DbContext - inherit from ZonitDbContext<T>
public class DatabaseContext : ZonitDbContext<DatabaseContext>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Your configuration here
        modelBuilder.Entity<Article>().ToTable("Articles");

        // ✅ Call base to enable Value Objects
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## :information_source: For more examples
See the `Examples` project included in the repository.
