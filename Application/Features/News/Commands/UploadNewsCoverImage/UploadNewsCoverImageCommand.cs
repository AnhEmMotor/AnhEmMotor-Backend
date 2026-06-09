using Application.Common.Models;
using MediatR;
using System.IO;
using System.Text.Json.Serialization;

namespace Application.Features.News.Commands.UploadNewsCoverImage;

public sealed record UploadNewsCoverImageResponse
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}

public sealed record UploadNewsCoverImageCommand : IRequest<Result<UploadNewsCoverImageResponse>>
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
}
