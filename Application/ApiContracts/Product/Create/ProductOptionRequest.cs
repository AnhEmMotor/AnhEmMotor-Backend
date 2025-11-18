using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Create;

public class ProductOptionRequest
{
    [Required]
    [StringLength(100)]
    public string? Name { get; set; }
}
