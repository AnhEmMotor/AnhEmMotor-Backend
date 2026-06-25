using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class CurrentVariantStoreResponse
    {
        public int Id { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("url_slug")]
        public string? UrlSlug { get; set; }

        public decimal? Price { get; set; }

        [JsonPropertyName("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [JsonPropertyName("colors")]
        public List<ProductVariantColorLiteResponse> Colors { get; set; } = [];

        [JsonPropertyName("product_limit")]
        public int? ProductLimit { get; set; }

        [JsonPropertyName("effectiveMax")]
        public int? EffectiveMax { get; set; }

        [JsonPropertyName("photo_collection")]
        public List<string> PhotoCollection { get; set; } = [];
    }
}
