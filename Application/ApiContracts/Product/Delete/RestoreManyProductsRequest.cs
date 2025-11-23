using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Delete;

public class RestoreManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

