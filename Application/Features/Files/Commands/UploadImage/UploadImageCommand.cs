using Application.ApiContracts.File;
using MediatR;

namespace Application.Features.Files.Commands.UploadImage;

public sealed record UploadImageCommand : IRequest<MediaFileResponse>
{
    public Stream? FileContent { get; init; }

    public string? FileName { get; init; }
}