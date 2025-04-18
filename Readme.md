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
- Built-in support for query building (`Where`, `Include`, `Select`, paging, ordering and more).
- Plug-and-play: Easily extend repositories with custom methods or external data without touching the extension itself.


## :bulb: **New! Extension Methods for External Data**
Now you can easily **include properties and data from outside your database**, for example, by pulling from an external API or service.  
This is useful when your model should have extra fields, computed properties, or needs to include data fetched live (not loaded from your DB).

### How it works:

1. **Create an Extension Class** implementing `IDatabaseExtension<TModel>`
    - In this class, implement business logic for fetching or computing the desired data.

```cs
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Extensions;

public class UserExtension : IDatabaseExtension<UserModel>
{
    public async Task<UserModel> InitializeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // EXAMPLE: Here we would call a REST API or any other source.
        var model = new UserModel { 
            Id = userId,
            Name = "UserName",
        };
        return await Task.FromResult(model);
    }
}
```

2. **Reference the extension using `.Extension()`** in your repository query chain.

```cs
var user = await _userRepository.Extension(x => x.UserExtension).GetAsync(userId);
```

The `.Extension(x => x.UserExtension)` call tells the repository to supply or load the `UserExtension` property for your entity.  
This can be `virtual`, `NotMapped`, or simply a property populated on demand.

**Use Case**:  
Suppose your `Blog` Entity has a `UserModel? User` property, but you want to always fetch the latest user data from an API instead of the DB.  
Simply create an extension and reference it. The fetching, mapping, and attach process is handled by the extension system for you.


## :book: Example Usage

### Entity Model

```cs
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
```cs
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

```cs
public interface IBlogRepository : IDatabaseRepository<Blog, Guid> { }

internal class BlogRepository(DatabaseContext _context)
    : DatabaseRepository<Blog, Guid>(_context), IBlogRepository { }

public interface IBlogsRepository : IDatabasesRepository<Blog> { }

internal class BlogsRepository(IDbContextFactory<DatabaseContext> _context)
    : DatabasesRepository<Blog, DatabaseContext>(_context), IBlogsRepository { }
```

### Service Registration / Dependency Injection

```cs
builder.Services.AddDbSqlServer<DatabaseContext>();

builder.Services.AddTransient<IBlogRepository, BlogRepository>();
builder.Services.AddTransient<IBlogsRepository, BlogsRepository>();
```

## :gear: CRUD Operations

### Create

```cs
var blog = await _blogRepository.AddAsync(new Blog
{
    Title = "Hello World",
    Content = "Example content"
});
```

### Read (Single or DTO)
```cs
var blogSingle = await _blogRepository.GetAsync(x => x.Title == "Hello World");
var blogSingleDto = await _blogRepository.GetAsync<BlogDto>(x => x.Title == "Hello World");
```

### Read First

```cs
var blogFirst = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
var blogFirstDto = await _blogRepository.GetFirstAsync<BlogDto>(x => x.Title == "Hello World");
``` 

Or:

```cs
var repository = _blogsRepository;
var blogs = await repository.OrderBy(x => x.Created).GetFirstAsync();
var blogsDto = await repository.OrderBy(x => x.Created).GetFirstAsync<BlogDto>();
```

### Update

```cs
var blog = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
blog.Title = "New Title";
var update = await _blogRepository.UpdateAsync(blog);
```

### Delete

```cs
var delete = await _blogRepository.DeleteAsync(blog.Id);
```

Or

```cs
var blog = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
var delete = await _blogRepository.DeleteAsync(blog);
```

### Read All

```cs
var repository = _blogsRepository;
var blogs = await repository.GetAsync();
var blogsDto = await repository.GetAsync<BlogDto>();
```

## :page_with_curl: Repository APIs

### IDatabaseRepository<TEntity, TType>

```cs
Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
Task<TDto> AddAsync<TDto>(TEntity entity, CancellationToken cancellationToken = default);

Task<TEntity?> GetAsync(TType id, CancellationToken cancellationToken = default);
Task<TDto?> GetAsync<TDto>(TType id, CancellationToken cancellationToken = default);

Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
Task<TDto?> GetAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
Task<TDto?> GetFirstAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(TType entity, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

IDatabaseReadRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension);

```

### IDatabasesRepository<TEntity>

```cs
IDatabasesRepository<TEntity> AsQuery();
IDatabasesRepository<TEntity> Extension(Expression<Func<TEntity, object?>> extension);
IDatabasesRepository<TEntity> Skip(int skip);
IDatabasesRepository<TEntity> Take(int take);
IDatabasesRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression);
IDatabasesRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
IDatabasesRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
IDatabasesRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
IDatabasesRepository<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);

Task<IReadOnlyCollection<TEntity>?> GetAsync(CancellationToken cancellationToken = default);
Task<IReadOnlyCollection<TDto>?> GetAsync<TDto>(CancellationToken cancellationToken = default);

Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);
Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);
Task<int> GetCountAsync(CancellationToken cancellationToken = default);
```


## :star2: Why use this extension?
- Build repository pattern code faster and cleaner.
- Decouple your database logic from your app logic.
- At any time, extend your repositories with custom business logic or fetch data from external APIs with zero changes to the extension library.
- Full control: you can always create custom methods in your repository when needed.


## :information_source: For more examples
See the `Examples` project included in the repository.
