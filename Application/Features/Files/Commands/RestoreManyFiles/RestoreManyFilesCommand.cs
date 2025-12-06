using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed record RestoreManyFilesCommand : IRequest<(List<ApiContracts.File.Responses.MediaFileResponse>? Data, ErrorResponse? Error)>
{
    public List<string> StoragePaths { get; init; } = [];
}
