using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.UploadProductImage;

public sealed record UploadProductImageCommand : IRequest<Result<MediaFileResponse>>
{
    public Stream? FileContent { get; init; }

    public string? FileName { get; init; }
}