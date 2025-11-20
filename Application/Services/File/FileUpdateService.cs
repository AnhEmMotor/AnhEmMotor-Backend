using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Entities;
using Domain.Helpers;

namespace Application.Services.File
{
    public class FileUpdateService(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileRestoreRepository mediaFileRestoreRepository, IUnitOfWork unitOfWork) : IFileUpdateService
    {
        public async Task<(FileResponse? Data, ErrorResponse? Error)> RestoreFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
            }

            try
            {
                var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                if (media is null)
                {
                    return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] });
                }

                var checkActive = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                if (checkActive is not null)
                {
                    return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File is not deleted." }] });
                }

                mediaFileRestoreRepository.Restore(media);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                var response = new FileResponse
                {
                    IsSuccess = true,
                    FileName = fileName,
                    Url = media.PublicUrl
                };
                return (response, null);
            }
            catch (Exception ex)
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Restore failed: {ex.Message}" }] });
            }
        }

        public async Task<(List<string>? Data, ErrorResponse? Error)> RestoreMultipleFilesAsync(List<string> fileNames, CancellationToken cancellationToken)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] });
            }

            var uniqueFileNames = fileNames.Distinct().ToList();

            var deleted = new List<MediaFile>();
            var notFound = new List<string>();

            foreach (var f in uniqueFileNames)
            {
                try
                {
                    var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                    if (media is null)
                    {
                        notFound.Add(f);
                        continue;
                    }

                    var isActive = await mediaFileSelectRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                    if (isActive is not null)
                    {
                        notFound.Add(f);
                        continue;
                    }

                    deleted.Add(media);
                }
                catch
                {
                    notFound.Add(f);
                }
            }

            if (notFound.Count > 0)
            {
                return (null, new ErrorResponse { Errors = [.. notFound.Select(n => new ErrorDetail { Message = $"File '{n}' not found or failed to restore." })] });
            }

            if (deleted.Count > 0)
            {
                mediaFileRestoreRepository.Restores(deleted);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return (uniqueFileNames, null);
        }
    }
}
