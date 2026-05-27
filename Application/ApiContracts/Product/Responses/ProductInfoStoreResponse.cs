using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductInfoStoreResponse
    {
        public string? Name { get; set; }

        public string? Brand { get; set; }

        public string? Category { get; set; }

        [JsonPropertyName("product_limit")]
        public int? ProductLimit { get; set; }

        public string? Description { get; set; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; set; }

        [JsonPropertyName("meta_title")]
        public string? MetaTitle { get; set; }

        [JsonPropertyName("meta_description")]
        public string? MetaDescription { get; set; }

        [JsonPropertyName("highlights")]
        public string? Highlights { get; set; }

        [JsonPropertyName("specifications")]
        public Dictionary<string, object?> Specifications { get; set; } = [];
    }
}
