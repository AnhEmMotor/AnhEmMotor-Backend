using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.News.Responses
{
    public sealed record UploadNewsContentImageResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; init; }

        [JsonPropertyName("data")]
        public UploadNewsContentImageData? Data { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }
}
