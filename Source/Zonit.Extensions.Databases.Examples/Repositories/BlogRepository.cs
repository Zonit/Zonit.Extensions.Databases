using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer.Repositories;

namespace Zonit.Extensions.Databases.Examples.Repositories;

internal class BlogRepository(DatabaseContext _context, IServiceProvider _serviceProvider) : DatabaseRepository<Blog, Guid>(_context, _serviceProvider), IBlogRepository
{
}