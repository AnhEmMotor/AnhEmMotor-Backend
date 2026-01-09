using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteFileCommand : IRequest<Common.Models.ErrorResponse?>
{
    public string StoragePath { get; init; } = string.Empty;
}
