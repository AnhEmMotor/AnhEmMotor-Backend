using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Delete;

public class DeleteManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

