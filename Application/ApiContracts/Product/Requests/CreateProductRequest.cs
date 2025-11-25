using Application.ApiContracts.Product.Common;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class CreateProductRequest : ProductWriteRequestBase
{
    public new string? Name { get; set; }

    [JsonPropertyName("category_id")]
    public new int? CategoryId { get; set; }

    [JsonPropertyName("brand_id")]
    public new int? BrandId { get; set; }

    [JsonPropertyName("status_id")]
    public new string? StatusId { get; set; }

    public new List<ProductVariantWriteRequest>? Variants { get; set; }
}
