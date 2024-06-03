using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Dto;

internal class BlogDto(Blog x)
{
    public string Id { get; set; } = $"Id: {x.Id}";
    public string Title { get; set; } = $"Title: {x.Title}";
    public string Content { get; set; } = $"Content: {x.Content}";
    public string Created { get; set; } = $"Created: {x.Created:G}";
}