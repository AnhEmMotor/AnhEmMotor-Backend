using Application.Common.Models;
using MediatR;
using System.IO;
using System.Text.Json.Serialization;

namespace Application.Features.News.Commands.UploadNewsContentImage;

public sealed record UploadNewsContentImageResponse
{
    [JsonPropertyName("errno")]
    public int Errno { get; init; }
    
    [JsonPropertyName("data")]
    public UploadNewsContentImageData? Data { get; init; }
    
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

public sealed record UploadNewsContentImageData
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
    
    [JsonPropertyName("alt")]
    public string Alt { get; init; } = string.Empty;
    
    [JsonPropertyName("href")]
    public string Href { get; init; } = string.Empty;
}

public sealed record UploadNewsContentImageCommand : IRequest<UploadNewsContentImageResponse>
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
}
