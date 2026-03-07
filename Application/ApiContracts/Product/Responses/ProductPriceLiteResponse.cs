namespace Application.ApiContracts.Product.Responses;

public class ProductPriceLiteResponse
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public List<ProductVariantPriceLiteResponse> Variants { get; set; } = [];
}
