using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class UpdateManyVariantPricesRequest
{
    public List<int>? Ids { get; set; }

    public long Price { get; set; }
}
