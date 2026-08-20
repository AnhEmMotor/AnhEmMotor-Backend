namespace Application.Features.ChatTools.Queries.SearchProductsForChat;

public record ChatProductSearchDto
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? BrandName { get; init; }

    public string? CategoryName { get; init; }

    public decimal? PriceFrom { get; init; }

    public decimal? PriceTo { get; init; }

    public string Currency { get; init; } = "VND";

    public int VariantCount { get; init; }

    public string? ImageUrl { get; init; }
}
