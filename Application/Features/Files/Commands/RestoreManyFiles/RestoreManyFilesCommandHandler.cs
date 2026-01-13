using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Files.Commands.RestoreManyFiles;

public sealed class RestoreManyFilesCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileUpdateRepository updateRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyFilesCommand, Result<List<MediaFileResponse>?>>
{
    public async Task<Result<List<MediaFileResponse>?>> Handle(
        RestoreManyFilesCommand request,
        CancellationToken cancellationToken)
    {
        var uniquePaths = request.StoragePaths.Distinct().ToList();
        var errorDetails = new List<Error>();

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
                errorDetails.Add(Error.NotFound($"File '{path}' not found.", "StoragePath"));
            } 
            else if(!deletedFileSet.Contains(path))
            {
                errorDetails.Add(Error.BadRequest($"File '{path}' is not deleted.", "StoragePath"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return errorDetails;
        }

        if(deletedFiles.ToList().Count > 0)
        {
            updateRepository.Restore(deletedFiles);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedFiles.Adapt<List<MediaFileResponse>>();
        foreach(var response in responses)
        {
            if(!string.IsNullOrEmpty(response.StoragePath))
            {
                response.PublicUrl = fileStorageService.GetPublicUrl(response.StoragePath);
            }
        }

        return responses;
    }
}
