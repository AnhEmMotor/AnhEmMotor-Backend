using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteFileCommand : IRequest<ErrorResponse?>
{
    public string StoragePath { get; init; } = string.Empty;
}
