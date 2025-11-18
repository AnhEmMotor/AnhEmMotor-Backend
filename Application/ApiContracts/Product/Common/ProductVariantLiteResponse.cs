namespace Application.ApiContracts.Product.Common;

public class ProductVariantLiteResponse
{
    public int? Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public long? Price { get; set; }
    public string? CoverImageUrl { get; set; }
    public long Stock { get; set; }
}

