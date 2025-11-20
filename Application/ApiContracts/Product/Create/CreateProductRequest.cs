using Application.ApiContracts.Product.Common;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Product.Create;

public class CreateProductRequest : ProductWriteRequestBase
{
    [Required]
    [StringLength(100)]
    public new string? Name { get; set; }

    [Required]
    public new int? CategoryId { get; set; }

    [Required]
    public new int? BrandId { get; set; }

    [Required]
    public new string? StatusId { get; set; }

    [MinLength(1)]
    public new List<ProductVariantWriteRequest>? Variants { get; set; }
}
