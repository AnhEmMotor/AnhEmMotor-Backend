using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Products.Responses
{
    public sealed record UploadProductContentImageResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; init; }

        [JsonPropertyName("data")]
        public UploadProductContentImageData? Data { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }
}
