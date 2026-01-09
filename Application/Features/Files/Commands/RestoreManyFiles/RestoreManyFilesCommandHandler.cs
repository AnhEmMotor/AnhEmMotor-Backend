using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed class RestoreManyFilesCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileUpdateRepository updateRepository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyFilesCommand, (List<ApiContracts.File.Responses.MediaFileResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<ApiContracts.File.Responses.MediaFileResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreManyFilesCommand request,
        CancellationToken cancellationToken)
    {
        if(request.StoragePaths == null || request.StoragePaths.Count == 0)
        {
            return ([], null);
        }

        var uniquePaths = request.StoragePaths.Distinct().ToList();
        var errorDetails = new List<Common.Models.ErrorDetail>();

        var allFiles = await readRepository.GetByStoragePathsAsync(uniquePaths, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var deletedFiles = await readRepository.GetByStoragePathsAsync(
            uniquePaths,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var allFileMap = allFiles.ToDictionary(f => f.StoragePath!);
        var deletedFileSet = deletedFiles.Select(f => f.StoragePath).ToHashSet();

        foreach(var path in uniquePaths)
        {
            if(!allFileMap.ContainsKey(path))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "StoragePath", Message = $"File '{path}' not found." });
            } else if(!deletedFileSet.Contains(path))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "StoragePath", Message = $"File '{path}' is not deleted." });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errorDetails });
        }

        if(deletedFiles.ToList().Count > 0)
        {
            updateRepository.Restore(deletedFiles);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedFiles.Adapt<List<ApiContracts.File.Responses.MediaFileResponse>>();
        foreach(var response in responses)
        {
            if(!string.IsNullOrEmpty(response.StoragePath))
            {
                response.PublicUrl = fileStorageService.GetPublicUrl(response.StoragePath);
            }
        }

        return (responses, null);
    }
}
