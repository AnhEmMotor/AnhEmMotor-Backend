using Application.ApiContracts.File;
using MediatR;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed record UploadManyImageCommand : IRequest<List<MediaFileResponse>>
{
    public List<FileUploadDto> Files { get; init; } = [];
}