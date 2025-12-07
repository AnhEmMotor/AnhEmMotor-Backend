using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed record RestoreFileCommand : IRequest<(ApiContracts.File.Responses.MediaFileResponse?, Common.Models.ErrorResponse?)>
{
    public string StoragePath { get; init; } = string.Empty;
}
