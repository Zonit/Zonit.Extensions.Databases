using Zonit.Extensions.Databases.Abstractions.Repositories;
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Repositories;

public interface IBlogsRepository : IDatabasesRepository<Blog>
{
}