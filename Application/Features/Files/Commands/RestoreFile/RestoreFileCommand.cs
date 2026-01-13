using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed record RestoreFileCommand : IRequest<Result<MediaFileResponse?>>
{
    public string StoragePath { get; init; } = string.Empty;
}
