using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteProductImageCommand : IRequest<Result>
{
    public string StoragePath { get; init; } = string.Empty;
}
