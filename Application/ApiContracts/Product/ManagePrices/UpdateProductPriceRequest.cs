using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.ManagePrices;

public class UpdateProductPriceRequest
{
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Giá phải là số dương.")]
    public long? Price { get; set; }
}
