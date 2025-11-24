using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed record RestoreFileCommand : IRequest<(MediaFileResponse?, ErrorResponse?)>
{
    public string StoragePath { get; init; } = string.Empty;
}
