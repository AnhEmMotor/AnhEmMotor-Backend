using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Helpers;

namespace Application.Services.File
{
    public class FileUpdateService(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileRestoreRepository mediaFileRestoreRepository) : IFileUpdateService
    {
        public async Task<ErrorResponse?> RestoreFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] };
            }

            try
            {
                var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                if (media is null)
                {
                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] };
                }

                var checkActive = await mediaFileSelectRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                if (checkActive is not null)
                {
                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File is not deleted." }] };
                }

                await mediaFileRestoreRepository.RestoreAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (Exception ex)
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Restore failed: {ex.Message}" }] };
            }
        }

        public async Task<ErrorResponse?> RestoreMultipleFilesAsync(List<string> fileNames, CancellationToken cancellationToken)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] };
            }

            var restored = new List<string>();
            var notFound = new List<string>();

            foreach (var f in fileNames)
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

                    await mediaFileRestoreRepository.RestoreAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
                    restored.Add(f);
                }
                catch
                {
                    notFound.Add(f);
                }
            }

            if (notFound.Count > 0)
            {
                return new ErrorResponse { Errors = [.. notFound.Select(n => new ErrorDetail { Message = $"File '{n}' not found or failed to restore." })] };
            }

            return null;
        }
    }
}
