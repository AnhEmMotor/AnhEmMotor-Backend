using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteManyFiles;

public sealed record DeleteManyFilesCommand : IRequest<ErrorResponse?>
{
    public List<string> StoragePaths { get; init; } = [];
}
