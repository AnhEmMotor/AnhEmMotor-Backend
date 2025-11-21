using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Create;

public class ProductOptionRequest
{
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Comma-separated list of option value IDs (e.g., "1,2,3")
    /// </summary>
    [Required]
    public string? Values { get; set; }
}
