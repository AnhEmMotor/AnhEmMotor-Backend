namespace Application.Features.ChatTools.Queries.ListNewsForChat;

public record ChatNewsListItemDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? CategoryName { get; init; }

    public DateTimeOffset? PublishedDate { get; init; }
}
