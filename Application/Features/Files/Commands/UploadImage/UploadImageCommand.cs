using Application.ApiContracts.File;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Files.Commands.UploadImage;

public sealed record UploadImageCommand : IRequest<MediaFileResponse>
{
    public IFormFile? File { get; init; }
}
