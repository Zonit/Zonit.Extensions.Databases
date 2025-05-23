﻿using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Repositories;

public interface IBlogRepository : IDatabaseRepository<Blog>
{
    Task GetCustomAsync();
}