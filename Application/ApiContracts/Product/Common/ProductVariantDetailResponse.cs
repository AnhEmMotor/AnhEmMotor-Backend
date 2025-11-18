using System;

namespace Application.ApiContracts.Product.Common;

public class ProductVariantDetailResponse
{
    public int? Id { get; set; }
    public int? ProductId { get; set; }
    public string? UrlSlug { get; set; }
    public long? Price { get; set; }
    public string? CoverImageUrl { get; set; }
    public Dictionary<string, string> OptionValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> PhotoCollection { get; set; } = [];
    public long Stock { get; set; }
    public long HasBeenBooked { get; set; }
    public string StatusStockId { get; set; } = string.Empty;
}

