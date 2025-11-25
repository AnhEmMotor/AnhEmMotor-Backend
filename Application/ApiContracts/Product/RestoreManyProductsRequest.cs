using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class RestoreManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

