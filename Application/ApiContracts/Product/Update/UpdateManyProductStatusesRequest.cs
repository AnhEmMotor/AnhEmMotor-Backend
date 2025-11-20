using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateManyProductStatusesRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 ID sản phẩm.")]
    public List<int>? Ids { get; set; }

    [Required]
    [InList("for-sale,out-of-business")]
    public string? StatusId { get; set; }
}
