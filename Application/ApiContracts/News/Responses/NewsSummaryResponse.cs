namespace Application.ApiContracts.News.Responses;

public sealed record NewsSummaryResponse
{
    public int Id { get; init; }

    public int? CategoryId { get; init; }

    public string? CategoryName { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? CoverImageUrl { get; init; }

    public string? AuthorName { get; init; }

    public string? MetaDescription { get; init; }

    public DateTimeOffset? PublishedDate { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public bool IsPublished { get; init; }
}
