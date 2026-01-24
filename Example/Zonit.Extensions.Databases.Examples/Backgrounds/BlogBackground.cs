using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zonit.Extensions.Databases.Examples.Dto;
using Zonit.Extensions.Databases.Examples.Entities;
using Zonit.Extensions.Databases.Examples.Repositories;

namespace Zonit.Extensions.Databases.Examples.Backgrounds;

internal class BlogBackground(
    IBlogRepository _blogRepository,
    ILogger<BlogBackground> _logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create
        var createBlog = await _blogRepository.AddAsync(new Blog
        {
            Title = "Hello World",
            Content = "Example content"
        }, stoppingToken);

        _logger.LogInformation("Create: {Id} {Title} {Content} {Created}", createBlog.Id, createBlog.Title, createBlog.Content, createBlog.Created);

        // Read - using Where (WhereFreeText requires full-text index on the table)
        var query = _blogRepository.AsQuery();
        query = query.Where(x => x.Title.Contains("Hello"));
        var read = await query.GetFirstAsync(stoppingToken);

        if (read is not null)
            _logger.LogInformation("Read: {Id} {Title} {Content} {Created}", read.Id, read.Title, read.Content, read.Created);
        else
            _logger.LogInformation("Blog not found");

        // Note: DTO mapping requires IMappingService registration
        // For production, register a mapping service (e.g., AutoMapper adapter)
        // var dto = await _blogRepository.Where(x => x.Title == "Hello World").GetFirstAsync<BlogDto>(stoppingToken);

        // Update
        if (read is not null)
        {
            read.Title = "New Title";
            var update = await _blogRepository.UpdateAsync(read, stoppingToken);

            if (update is not null)
                _logger.LogInformation("Blog updated: {Title}", update.Title);
            else
                _logger.LogInformation("Blog not updated");
        }

        // Delete
        var delete = await _blogRepository.DeleteAsync(createBlog, cancellationToken: stoppingToken);

        if (delete is true)
            _logger.LogInformation("Blog deleted");
        else
            _logger.LogInformation("Blog not deleted");
    }
}