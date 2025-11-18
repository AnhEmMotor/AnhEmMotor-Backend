using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Delete;

public class RestoreManyProductsRequest
{
    [MinLength(1)]
    public List<int> Ids { get; set; } = [];
}

