using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateProductPriceRequest
{
    public long? Price { get; set; }
}
