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
        });

        _logger.LogInformation("Create: {Id} {Title} {Content} {Created}", createBlog.Id, createBlog.Title, createBlog.Content, createBlog.Created);

        // Read
        var read = await _blogRepository.GetFirstAsync(x => x.Title == "Hello World");

        if (read is not null)
            _logger.LogInformation("Read: {Id} {Title} {Content} {Created}", read.Id, read.Title, read.Content, read.Created);
        else
            _logger.LogInformation("Blog not found");

        // Dto Read
        var dto = await _blogRepository.GetFirstAsync<BlogDto>(x => x.Title == "Hello World");

        if (dto is not null)
            _logger.LogInformation("Dto Read: {Id} {Title} {Content} {Created}", dto.Id, dto.Title, dto.Content, dto.Created);
        else
            _logger.LogInformation("Blog not found");

        // Update
        if (read is not null)
        {
            read.Title = "New Title";
            var update = await _blogRepository.UpdateAsync(read);

            if(update is true)
                _logger.LogInformation("Blog updated");
            else
                _logger.LogInformation("Blog not updated");
        }

        // Delete
        var delete = await _blogRepository.DeleteAsync(createBlog);

        if(delete is true)
            _logger.LogInformation("Blog deleted");
        else
            _logger.LogInformation("Blog not deleted");
    }
}