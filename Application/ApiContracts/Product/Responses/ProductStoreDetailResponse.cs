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

public class ProductInfoStoreResponse
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }
    
    [JsonExtensionData]
    public Dictionary<string, object?> Specifications { get; set; } = [];
}

public class CurrentVariantStoreResponse
{
    public int Id { get; set; }
    
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    
    public decimal? Price { get; set; }
    
    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }
    
    [JsonPropertyName("photo_collection")]
    public List<string> PhotoCollection { get; set; } = [];
}

public class OtherVariantStoreResponse
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    
    public string? Slug { get; set; }
    
    public decimal? Price { get; set; }
}
