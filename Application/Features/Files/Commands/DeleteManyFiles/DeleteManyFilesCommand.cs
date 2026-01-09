
using MediatR;

namespace Application.Features.Files.Commands.DeleteManyFiles;

public sealed record DeleteManyFilesCommand : IRequest<Common.Models.ErrorResponse?>
{
    public List<string> StoragePaths { get; init; } = [];
}
