using Application.ApiContracts.File;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed record UploadManyImageCommand : IRequest<List<MediaFileResponse>>
{
    public List<IFormFile> Files { get; init; } = [];
}
