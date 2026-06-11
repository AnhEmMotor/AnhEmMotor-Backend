using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Commands.UploadNewsCoverImage;

public sealed record UploadNewsCoverImageCommand : IRequest<Result<UploadNewsCoverImageResponse>>
{
    public Stream FileStream { get; init; } = Stream.Null;

    public string FileName { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;
}
