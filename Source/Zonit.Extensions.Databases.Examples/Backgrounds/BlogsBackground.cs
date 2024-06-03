using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zonit.Extensions.Databases.Examples.Dto;
using Zonit.Extensions.Databases.Examples.Repositories;

namespace Zonit.Extensions.Databases.Examples.Backgrounds;

internal class BlogsBackground(
    IBlogsRepository _blogsRepository,
    ILogger<BlogsBackground> _logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Update range
        using (var repository = _blogsRepository)
        {
            var count = await _blogsRepository.Where(x => x.Created > DateTime.Now.AddYears(-1)).UpdateRangeAsync(x => {
                x.Title = "New all title";
                x.Content = "New all content";
            }, stoppingToken);

            if (count is not null)
                _logger.LogInformation("Updated {Count} blogs", count);
        }

        // Read
        using (var repository = _blogsRepository)
        {
            var blogs = await repository.GetAsync<BlogDto>(stoppingToken);

            if(blogs is not null)
                foreach (var blog in blogs)
                    _logger.LogInformation("Blog: {Id} {Title} {Content} {Created}", blog.Id, blog.Title, blog.Content, blog.Created);
            else
                _logger.LogInformation("Blogs not found");
        }
    }
}