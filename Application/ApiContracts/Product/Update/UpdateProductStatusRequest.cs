using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Update;

public class UpdateProductStatusRequest
{
    [Required]
    [InList("for-sale,out-of-business")]
    public string? StatusId { get; set; }
}
