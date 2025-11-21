using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Create;

public class ProductOptionRequest
{
    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Comma-separated list of option values (e.g., "Small,Medium,Large")
    /// </summary>
    [Required]
    public string? Values { get; set; }
}
