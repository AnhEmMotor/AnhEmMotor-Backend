using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductVariantDetailResponse
    {
        public int? Id { get; set; }

        [JsonPropertyName("product_id")]
        public int? ProductId { get; set; }

        [JsonPropertyName("url")]
        public string? UrlSlug { get; set; }

        public long? Price { get; set; }

        [JsonPropertyName("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [JsonPropertyName("optionValues")]
        public Dictionary<string, string> OptionValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("photo_collection")]
        public List<string> PhotoCollection { get; set; } = [];
    }
}
