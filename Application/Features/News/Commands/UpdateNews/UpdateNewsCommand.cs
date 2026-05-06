using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.News.Commands.UpdateNews;

public sealed record UpdateNewsCommand : IRequest<Result<Unit>>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Content { get; init; }
    
    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; init; }
    
    [JsonPropertyName("author_name")]
    public string? AuthorName { get; init; }
    
    [JsonPropertyName("is_published")]
    public bool IsPublished { get; init; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; init; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; init; }

    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; init; }
}
