using Application.ApiContracts.News.Responses;
using MediatR;

namespace Application.Features.News.Commands.UploadNewsContentImage;

public sealed record UploadNewsContentImageCommand : IRequest<UploadNewsContentImageResponse>
{
    public Stream FileStream { get; init; } = Stream.Null;

    public string FileName { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;
}
