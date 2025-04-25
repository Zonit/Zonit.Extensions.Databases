using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zonit.Extensions.Databases.Examples.Dto;
using Zonit.Extensions.Databases.Examples.Repositories;

namespace Zonit.Extensions.Databases.Examples.Backgrounds;

internal class BlogsBackground(
    IBlogRepository _blogsRepository,
    ILogger<BlogsBackground> _logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);

        // Update range
        var count = await _blogsRepository.Where(x => x.Created > DateTime.Now.AddYears(-1)).UpdateRangeAsync(x => {
            x.Title = "New all title";
            x.Content = "New all content";
        }, stoppingToken);

        if (count is not null)
            _logger.LogInformation("Updated {Count} blogs", count);


        // Read
        var blogs = await _blogsRepository
            .Extension(x => x.User)
            .GetListAsync<BlogDto>(stoppingToken);

        if(blogs is not null)
            foreach (var blog in blogs)
            {
                _logger.LogInformation("Blog: {User} {Id} {Title} {Content} {Created}", blog.User, blog.Id, blog.Title, blog.Content, blog.Created);
            }
        else
            _logger.LogInformation("Blogs not found");
        

        // Read first
        var blogFirst = await _blogsRepository
            .OrderBy(x => x.Id)
            .GetFirstAsync<BlogDto>(stoppingToken);

        if (blogFirst is not null)
            _logger.LogInformation("First blog: {Id} {Title} {Content} {Created}", blogFirst.Id, blogFirst.Title, blogFirst.Content, blogFirst.Created);
        else
            _logger.LogInformation("Blog not found");
        
    }
}