namespace Application.ApiContracts.Product.Responses;

public class VariantCartDetailResponse
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? CoverImageUrl { get; set; }
}
