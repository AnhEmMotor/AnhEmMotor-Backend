using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.News.Responses
{
    public sealed record UploadNewsContentImageData
    {
        [JsonPropertyName("url")]
        public string Url { get; init; } = string.Empty;

        [JsonPropertyName("alt")]
        public string Alt { get; init; } = string.Empty;

        [JsonPropertyName("href")]
        public string Href { get; init; } = string.Empty;
    }
}
