namespace Application.ApiContracts.Product.Update;

public class UpdateManyVariantPricesRequest
{
    public Dictionary<int, long> VariantPrices { get; set; } = [];
}
