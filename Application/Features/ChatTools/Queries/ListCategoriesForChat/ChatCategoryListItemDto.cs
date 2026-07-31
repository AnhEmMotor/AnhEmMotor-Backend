namespace Application.Features.ChatTools.Queries.ListCategoriesForChat;

public record ChatCategoryListItemDto
{
    public string CategoryName { get; init; } = string.Empty;

    public int ProductCount { get; init; }
}
