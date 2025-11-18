using Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.ManageStatus;

public class UpdateProductStatusRequest
{
    [Required]
    [InList("for-sale,out-of-business")]
    public string? StatusId { get; set; }
}
