using Microsoft.EntityFrameworkCore;
using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer.Repositories;

namespace Zonit.Extensions.Databases.Examples.Repositories;

internal class BlogsRepository(IDbContextFactory<DatabaseContext> _context, IServiceProvider _serviceProvider) : DatabasesRepository<Blog, DatabaseContext>(_context, _serviceProvider), IBlogsRepository
{
}