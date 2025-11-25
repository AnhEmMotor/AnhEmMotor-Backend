namespace Application.ApiContracts.Product.Requests;

public class RestoreManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

