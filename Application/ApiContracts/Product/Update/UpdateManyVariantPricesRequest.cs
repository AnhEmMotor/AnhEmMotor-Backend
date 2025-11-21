using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateManyVariantPricesRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 variant.")]
    public List<int>? Ids { get; set; }

    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
    public long Price { get; set; }
}
