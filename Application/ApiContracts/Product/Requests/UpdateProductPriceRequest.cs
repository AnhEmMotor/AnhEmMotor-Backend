using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product;

public class UpdateProductPriceRequest
{
    public long? Price { get; set; }
}
