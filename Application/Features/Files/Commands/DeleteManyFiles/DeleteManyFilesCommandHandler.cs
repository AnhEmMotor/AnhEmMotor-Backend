using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;

using Domain.Constants;
using MediatR;

namespace Application.Features.Files.Commands.DeleteManyFiles;

public sealed class DeleteManyFilesCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyFilesCommand, Result>
{
    public async Task<Result> Handle(DeleteManyFilesCommand request, CancellationToken cancellationToken)
    {
        if(request.StoragePaths == null || request.StoragePaths.Count == 0)
        {
            return Result.Failure("You not pass storage path to delete");
        }

        var uniquePaths = request.StoragePaths.Distinct().ToList();
        var errorDetails = new List<Error>();

        var allFiles = await readRepository.GetByStoragePathsAsync(uniquePaths, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeFiles = await readRepository.GetByStoragePathsAsync(uniquePaths, cancellationToken)
            .ConfigureAwait(false);

        var allFileMap = allFiles.ToDictionary(f => f.StoragePath!);
        var activeFileSet = activeFiles.Select(f => f.StoragePath).ToHashSet();

        foreach(var path in uniquePaths)
        {
            if(!allFileMap.ContainsKey(path))
            {
                errorDetails.Add(Error.NotFound($"File '{path}' not found.", "StoragePath"));
            } else if(!activeFileSet.Contains(path))
            {
                errorDetails.Add(Error.BadRequest($"File '{path}' has already been deleted.", "StoragePath"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(activeFiles.ToList().Count > 0)
        {
            deleteRepository.Delete(activeFiles);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
