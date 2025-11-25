namespace Application.ApiContracts.Product.Requests;

public class DeleteManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

