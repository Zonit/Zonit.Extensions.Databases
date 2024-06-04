## The extension facilitates CRUD creation in databases.

It speeds up the work of creating repositories, with the help of abstractions and interface handle the connection to the database.

**Nuget Package Abstraction**
```
Install-Package Zonit.Extensions.Databases.Abstractions 
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Databases.Abstractions.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Databases.Abstractions.svg)

**Nuget Package Extensions SqlServer**
```
Install-Package Zonit.Extensions.Databases.SqlServer
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Databases.SqlServer.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Databases.SqlServer.svg)

### Example:
**Model**
```cs
public class Blog
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; private set; } = DateTime.UtcNow;
}

internal class BlogDto(Blog x)
{
    public string Id { get; set; } = $"Id: {x.Id}";
    public string Title { get; set; } = $"Title: {x.Title}";
    public string Content { get; set; } = $"Content: {x.Content}";
    public string Created { get; set; } = $"Created: {x.Created:G}";
}
```

**Repository**
```cs
public interface IBlogRepository : IDatabaseRepository<Blog, Guid>
{ }

internal class BlogRepository(DatabaseContext _context) : DatabaseRepository<Blog, Guid>(_context), IBlogRepository
{ }

public interface IBlogsRepository : IDatabasesRepository<Blog>
{ }

internal class BlogsRepository(IDbContextFactory<DatabaseContext> _context) : DatabasesRepository<Blog, DatabaseContext>(_context), IBlogsRepository
{ }
```

**Register**
```cs
    builder.Services.AddDbSqlServer<DatabaseContext>();

    builder.Services.AddTransient<IBlogRepository, BlogRepository>();
    builder.Services.AddTransient<IBlogsRepository, BlogsRepository>();
```

**Create**
```cs
var blog = await _blogRepository.AddAsync(new Blog
{
    Title = "Hello World",
    Content = "Example content"
});
```

**Read single**
```cs
var blogSingle = await _blogRepository.GetAsync(x => x.Title == "Hello World");
var blogSingleDto = await _blogRepository.GetAsync<BlogDto>(x => x.Title == "Hello World");
```

**Read first**
```cs
var blogFirst = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
var blogFirstDto = await _blogRepository.GetFirstAsync<BlogDto>(x => x.Title == "Hello World");
```
or
```cs
using var repository = _blogsRepository;
var blogs = await repository.OrderBy(x => x.Created).GetFirstAsync();
var blogsDto = await repository.OrderBy(x => x.Created).GetFirstAsync<BlogDto>();
```

**Update**
```cs
var blog = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
blog.Title = "New Title";
var update = await _blogRepository.UpdateAsync(blog);
```

**Delete**
```cs
var delete = await _blogRepository.DeleteAsync(blog.Id);
```
or
```cs
var blog = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");
var delete = await _blogRepository.DeleteAsync(blog);
```
**Read All**
```cs
using var repository = _blogsRepository;
var blogs = await repository.GetAsync();
var blogsDto = await repository.GetAsync<BlogDto>();
```

**API IDatabaseRepository<TEntity, TType>**
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
```

**API IDatabasesRepository\<TEntity>**
```cs
IDatabasesRepository<TEntity> Skip(int skip);
IDatabasesRepository<TEntity> Take(int take);
IDatabasesRepository<TEntity> Include(Expression<Func<TEntity, object>> includeExpression);
IDatabasesRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
IDatabasesRepository<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
IDatabasesRepository<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
IDatabasesRepository<TEntity> Select(Expression<Func<TEntity, TEntity>> selector);

/// <summary>
/// Returns a list of available results 
/// </summary>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<IReadOnlyCollection<TEntity>?> GetAsync(CancellationToken cancellationToken = default);

/// <summary>
/// Returns a list of available results by changing them to DTOs
/// </summary>
/// <typeparam name="TDto"></typeparam>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<IReadOnlyCollection<TDto>?> GetAsync<TDto>(CancellationToken cancellationToken = default);

/// <summary>
/// Returns a single result
/// </summary>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<TEntity?> GetFirstAsync(CancellationToken cancellationToken = default);

/// <summary>
/// Returns a single result by changing it to DTO
/// </summary>
/// <typeparam name="TDto"></typeparam>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<TDto?> GetFirstAsync<TDto>(CancellationToken cancellationToken = default);

/// <summary>
/// Update multiple data
/// </summary>
/// <param name="predicate">Data to be changed</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<int?> UpdateRangeAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default);

/// <summary>
/// Get the number of available results
/// </summary>
/// <param name="cancellationToken"></param>
/// <returns></returns>
Task<int> GetCountAsync(CancellationToken cancellationToken = default);
```

For more information, see the Examples project.