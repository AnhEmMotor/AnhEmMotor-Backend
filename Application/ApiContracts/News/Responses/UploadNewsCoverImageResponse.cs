using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.News.Responses
{
    public sealed record UploadNewsCoverImageResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; init; } = string.Empty;
    }
}
