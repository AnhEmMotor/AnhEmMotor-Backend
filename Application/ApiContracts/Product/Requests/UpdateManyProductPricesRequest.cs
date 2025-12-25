namespace Application.ApiContracts.Product.Requests;

public class UpdateManyProductPricesRequest
{
    public List<int>? Ids { get; set; }

    public decimal Price { get; set; }
}
