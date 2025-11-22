using Application.ApiContracts.Product.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Create;

public class CreateProductRequest : ProductWriteRequestBase
{
    [Required]
    [StringLength(100)]
    public new string? Name { get; set; }

    [Required]
    [JsonPropertyName("category_id")]
    public new int? CategoryId { get; set; }

    [Required]
    [JsonPropertyName("brand_id")]
    public new int? BrandId { get; set; }

    [Required]
    [JsonPropertyName("status_id")]
    public new string? StatusId { get; set; }

    [MinLength(1)]
    public new List<ProductVariantWriteRequest>? Variants { get; set; }
}
