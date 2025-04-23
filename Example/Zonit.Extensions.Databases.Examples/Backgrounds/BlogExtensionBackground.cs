using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.Examples.Repositories;

namespace Zonit.Extensions.Databases.Examples.Backgrounds;

internal class BlogExtensionBackground(
    IBlogRepository _blogRepository,
    ILogger<BlogExtensionBackground> _logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);

        // Create
        var createBlog = await _blogRepository.AddAsync(new Blog
        {
            Title = "Hello World",
            Content = "Example content",
            UserId = Guid.NewGuid()
        }, stoppingToken);

        _logger.LogInformation("Create: {Id} {Title} {Content} {Created}", createBlog.Id, createBlog.Title, createBlog.Content, createBlog.Created);

        // Read
        var read = await _blogRepository
            .Extension(x => x.User)
            .GetByIdAsync(createBlog.Id, stoppingToken);

        if (read is not null)
        {
            _logger.LogInformation("Read: {Id} {Title} {Content} {Created}", read.Id, read.Title, read.Content, read.Created);
            _logger.LogInformation("User: {Name}", read.User?.Name);
        }
        else
            _logger.LogInformation("Blog not found");


        // Read
        var read2 = await _blogRepository
            .Extension(x => x.User)
            .Where(x => x.Id != createBlog.Id)
            .GetAsync(stoppingToken);

        if (read2 is not null)
        {
            _logger.LogInformation("Read: {Id} {Title} {Content} {Created}", read2.Id, read2.Title, read2.Content, read2.Created);
            _logger.LogInformation("User: {Name}", read2.User?.Name);
        }
        else
            _logger.LogInformation("Blog not found");


        await _blogRepository.GetCustomAsync();
    }
}