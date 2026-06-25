using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Products.Responses
{
    public sealed record UploadProductContentImageData
    {
        [JsonPropertyName("url")]
        public string Url { get; init; } = string.Empty;

        [JsonPropertyName("alt")]
        public string Alt { get; init; } = string.Empty;

        [JsonPropertyName("href")]
        public string Href { get; init; } = string.Empty;
    }
}
