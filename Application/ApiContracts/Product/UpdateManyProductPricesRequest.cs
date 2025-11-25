using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class UpdateManyProductPricesRequest
{
    public List<int>? Ids { get; set; }

    public long Price { get; set; }
}
