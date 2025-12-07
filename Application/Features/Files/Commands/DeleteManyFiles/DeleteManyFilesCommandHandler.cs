using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using MediatR;

namespace Application.Features.Files.Commands.DeleteManyFiles;

public sealed class DeleteManyFilesCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyFilesCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteManyFilesCommand request,
        CancellationToken cancellationToken)
    {
        if(request.StoragePaths == null || request.StoragePaths.Count == 0)
        {
            return null;
        }

        var uniquePaths = request.StoragePaths.Distinct().ToList();
        var errorDetails = new List<Common.Models.ErrorDetail>();

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
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "StoragePath", Message = $"File '{path}' not found." });
            } else if(!activeFileSet.Contains(path))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "StoragePath",
                        Message = $"File '{path}' has already been deleted."
                    });
            }
        }

        if(errorDetails.Count > 0)
        {
            return new Common.Models.ErrorResponse { Errors = errorDetails };
        }

        if(activeFiles.ToList().Count > 0)
        {
            deleteRepository.Delete(activeFiles);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
