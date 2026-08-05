namespace Application.Features.ChatTools.Queries.GetRevenueByCategoryForChat;

public record ChatRevenueByCategoryItemDto
{
    public string CategoryName { get; init; } = string.Empty;

    public decimal Revenue { get; init; }

    public decimal Percentage { get; init; }
}
