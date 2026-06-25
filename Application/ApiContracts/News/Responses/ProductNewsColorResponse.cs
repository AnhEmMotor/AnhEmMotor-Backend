using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.News.Responses
{
    public class ProductNewsColorResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("img")]
        public string Img { get; set; } = string.Empty;
    }
}
