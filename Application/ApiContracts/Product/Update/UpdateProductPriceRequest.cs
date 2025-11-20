using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateProductPriceRequest
{
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Giá phải là số dương.")]
    public long? Price { get; set; }
}
