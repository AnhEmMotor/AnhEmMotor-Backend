using Domain.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed record RestoreManyFilesCommand : IRequest<(List<ApiContracts.File.Responses.MediaFileResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public List<string> StoragePaths { get; init; } = [];
}
