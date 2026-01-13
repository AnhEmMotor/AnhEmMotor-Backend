using MediatR;
using Application.Common.Models;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteFileCommand : IRequest<Result>
{
    public string StoragePath { get; init; } = string.Empty;
}
