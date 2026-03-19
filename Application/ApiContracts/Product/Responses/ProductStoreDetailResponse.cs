using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses;

public class ProductStoreDetailResponse
{
    public ProductInfoStoreResponse Product { get; set; } = null!;

    [JsonPropertyName("current_variant")]
    public CurrentVariantStoreResponse CurrentVariant { get; set; } = null!;

    [JsonPropertyName("other_variants")]
    public List<OtherVariantStoreResponse> OtherVariants { get; set; } = [];
}


