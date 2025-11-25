using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class DeleteManyProductsRequest
{
    public List<int> Ids { get; set; } = [];
}

