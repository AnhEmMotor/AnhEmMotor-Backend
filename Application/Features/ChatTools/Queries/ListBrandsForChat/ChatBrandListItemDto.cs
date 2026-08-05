namespace Application.Features.ChatTools.Queries.ListBrandsForChat;

public record ChatBrandListItemDto
{
    public string BrandName { get; init; } = string.Empty;

    public string? Origin { get; init; }
}
