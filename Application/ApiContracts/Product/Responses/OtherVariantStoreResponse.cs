using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class OtherVariantStoreResponse
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        public string? Slug { get; set; }

        public decimal? Price { get; set; }
    }
}
