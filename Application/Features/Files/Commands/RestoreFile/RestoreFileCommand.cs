using Application.ApiContracts.File.Responses;
using MediatR;
using Application.Common.Models;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed record RestoreFileCommand : IRequest<Result<MediaFileResponse?>>
{
    public string StoragePath { get; init; } = string.Empty;
}
