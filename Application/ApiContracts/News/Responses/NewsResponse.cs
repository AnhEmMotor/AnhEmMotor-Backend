namespace Application.ApiContracts.News.Responses;

public sealed record NewsResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Content { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? AuthorName { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public bool IsPublished { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
