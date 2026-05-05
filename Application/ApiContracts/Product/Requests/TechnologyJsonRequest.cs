using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests
{
    public class TechnologyJsonRequest
    {
        [JsonPropertyName("technologyId")]
        public int TechnologyId { get; set; }

        [JsonPropertyName("customTitle")]
        public string? CustomTitle { get; set; }

        [JsonPropertyName("customDescription")]
        public string? CustomDescription { get; set; }

        [JsonPropertyName("customImageUrl")]
        public string? CustomImageUrl { get; set; }
    }
}
