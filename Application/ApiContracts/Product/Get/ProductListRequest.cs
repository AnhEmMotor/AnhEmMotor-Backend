namespace Application.ApiContracts.Product.Get;

public class ProductListRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
    public List<string> StatusIds { get; set; } = [];
}

