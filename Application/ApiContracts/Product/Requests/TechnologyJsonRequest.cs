using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests
{
    public class TechnologyJsonRequest
    {
        [JsonPropertyName("technology_id")]
        public int TechnologyId { get; set; }

        [JsonPropertyName("custom_title")]
        public string? CustomTitle { get; set; }

        [JsonPropertyName("custom_description")]
        public string? CustomDescription { get; set; }

        [JsonPropertyName("custom_image_url")]
        public string? CustomImageUrl { get; set; }
    }
}
