namespace Application.Features.ChatTools.Queries.GetProductDetailForChat;

public record ChatProductDetailDto
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? ShortDescription { get; init; }

    public string? BrandName { get; init; }

    public string? CategoryName { get; init; }

    public decimal? PriceFrom { get; init; }

    public decimal? PriceTo { get; init; }

    public string Currency { get; init; } = "VND";

    public IReadOnlyList<ChatProductVariantDetailDto> Variants { get; init; } = [];
}

public record ChatProductVariantDetailDto
{
    public int VariantId { get; init; }

    public string? VariantName { get; init; }

    public string? Sku { get; init; }

    public decimal? Price { get; init; }

    public string? Slug { get; init; }

    public IReadOnlyList<ChatVariantColorDto> Colors { get; init; } = [];
}

public record ChatVariantColorDto
{
    public int ColorId { get; init; }

    public string? ColorName { get; init; }

    public string? ColorCode { get; init; }

    public string? ImageUrl { get; init; }
}
