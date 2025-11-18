namespace Application.ApiContracts.Product.Common;

public class ProductOptionDetailResponse
{
    public string? Name { get; set; }
    public List<string> Values { get; set; } = [];
}

