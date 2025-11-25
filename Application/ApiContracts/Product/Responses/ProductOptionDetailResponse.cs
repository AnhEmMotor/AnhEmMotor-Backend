namespace Application.ApiContracts.Product.Responses;

public class ProductOptionDetailResponse
{
    public string? Name { get; set; }

    public List<string> Values { get; set; } = [];
}

