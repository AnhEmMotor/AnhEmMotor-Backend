namespace Application.ApiContracts.News.Responses;

public record NewsForStoreResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = default!;

    public string Content { get; init; } = default!;

    public string CategoryName { get; init; } = default!;

    public string CoverImageUrl { get; init; } = default!;

    public DateTimeOffset? PublishedDate { get; init; }

    public string? Excerpt { get; init; }

    public List<NewsProductForStoreResponse> LinkedProducts { get; init; } = [];
}

