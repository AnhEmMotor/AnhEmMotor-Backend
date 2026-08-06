namespace Application.DTOs.StoreChat;

public class StoreChatProductSearchItemDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal? PriceFrom { get; set; }

    public decimal? PriceTo { get; set; }
}

public class StoreChatVariantCardDto
{
    public int VariantId { get; set; }

    public string? VariantName { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public decimal? Price { get; set; }

    public string? Slug { get; set; }

    public List<StoreChatVariantColorDto> Colors { get; set; } = [];
}

public class StoreChatVariantColorDto
{
    public int ColorId { get; set; }

    public string? ColorName { get; set; }

    public string? ColorCode { get; set; }

    public string? ImageUrl { get; set; }
}
