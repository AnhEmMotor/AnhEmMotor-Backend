using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.DeleteManyFiles;

public sealed record DeleteManyFilesCommand : IRequest<Result>
{
    public List<string> StoragePaths { get; init; } = [];
}
