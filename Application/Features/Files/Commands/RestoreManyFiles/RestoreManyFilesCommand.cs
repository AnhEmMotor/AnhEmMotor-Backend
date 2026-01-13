using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed record RestoreManyFilesCommand : IRequest<Result<List<MediaFileResponse>?>>
{
    public List<string> StoragePaths { get; init; } = [];
}
