using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Entities;
using Domain.Helpers;

namespace Application.Services.File
{
    public class FileDeleteService(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileDeleteRepository mediaFileDeleteRepository, IUnitOfWork unitOfWork) : IFileDeleteService
    {
        public async Task<ErrorResponse?> DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] };
            }
            try
            {
                var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);

                if (media is null)
                {
                    var existed = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                    if (existed is not null)
                    {
                        return new ErrorResponse { Errors = [new ErrorDetail { Message = "File already deleted." }] };
                    }

                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] };
                }

                mediaFileDeleteRepository.Delete(media);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (Exception ex)
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Delete failed: {ex.Message}" }] };
            }
        }

        public async Task<ErrorResponse?> DeleteMultipleFilesAsync(List<string?> fileNames, CancellationToken cancellationToken)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return null;
            }

            var uniqueFileNames = fileNames.Distinct().ToList();
            var errorDetails = new List<ErrorDetail>();

            List<MediaFile>? allMediaFile = await mediaFileSelectRepository.GetByStoredFileNamesAsync(fileNames, cancellationToken, true).ConfigureAwait(false);
            List<MediaFile>? activeMediaFile = await mediaFileSelectRepository.GetByStoredFileNamesAsync(fileNames, cancellationToken, false).ConfigureAwait(false);

            var allMediaFileMap = allMediaFile!.ToDictionary(b => b.StoredFileName!);
            var activeMediaFileSet = activeMediaFile!.Select(b => b.StoredFileName!).ToHashSet();

            foreach (var fileName in uniqueFileNames)
            {
                if (!allMediaFileMap.ContainsKey(fileName!))
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = $"File '{fileName}' not found."
                    });
                }

                if (!activeMediaFileSet.Contains(fileName!))
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"File '{fileName}' has already been deleted"
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (uniqueFileNames.Count > 0)
            {
                mediaFileDeleteRepository.DeleteRange(allMediaFile!);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
