namespace Application.ApiContracts.Product.Requests;

public class UpdateManyVariantPricesRequest
{
    public List<int>? Ids { get; set; }

    public decimal Price { get; set; }
}
