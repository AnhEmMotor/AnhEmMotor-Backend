using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateManyProductPricesRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm.")]
    public Dictionary<string, long>? ProductPrices { get; set; }
}
