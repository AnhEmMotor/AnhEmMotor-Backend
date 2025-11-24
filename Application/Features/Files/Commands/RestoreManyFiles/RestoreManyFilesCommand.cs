using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed record RestoreManyFilesCommand : IRequest<(List<MediaFileResponse>? Data, ErrorResponse? Error)>
{
    public List<string> StoragePaths { get; init; } = [];
}
