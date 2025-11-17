using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Helpers;

namespace Application.Services.File
{
    public class FileDeleteService(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileDeleteRepository mediaFileDeleteRepository) : IFileDeleteService
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

                await mediaFileDeleteRepository.DeleteAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (Exception ex)
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Delete failed: {ex.Message}" }] };
            }
        }

        public async Task<ErrorResponse?> DeleteMultipleFilesAsync(List<string> fileNames, CancellationToken cancellationToken)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] };
            }

            var deleted = new List<string>();
            var notFound = new List<string>();

            foreach (var f in fileNames)
            {
                try
                {
                    var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                    if (media is null)
                    {
                        var existed = await mediaFileSelectRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                        if (existed is not null)
                        {
                            notFound.Add(f);
                            continue;
                        }

                        notFound.Add(f);
                        continue;
                    }

                    await mediaFileDeleteRepository.DeleteAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
                    deleted.Add(f);
                }
                catch
                {
                    notFound.Add(f);
                }
            }

            if (notFound.Count > 0)
            {
                return new ErrorResponse { Errors = [.. notFound.Select(n => new ErrorDetail { Message = $"File '{n}' not found or failed to delete." })] };
            }

            return null;
        }
    }
}
