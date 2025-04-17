using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.SqlServer;
using Zonit.Extensions.Databases.SqlServer.Repositories;

namespace Zonit.Extensions.Databases.Examples.Repositories;

internal class BlogRepository(Context<DatabaseContext> context) : DatabaseRepository<Blog>(context), IBlogRepository
{
}