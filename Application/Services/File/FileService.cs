using Application.ApiContracts.File;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Services.File
{
    public class FileService(IFileRepository fileRepository, IMediaFileRepository mediaFileRepository) : IFileService
    {
        private readonly string[] _permittedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
        private readonly Dictionary<string, string> _mimeTypeMappings = new()
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" }
        };

        private async Task<(MediaFile? FileData, UploadResponse? Response, ErrorResponse? Error)> ProcessAndSaveFileToDiskAsync(
            IFormFile file, string baseUrl, CancellationToken cancellationToken)
        {
            var (isValid, validationError) = await ValidateFileAsync(file, cancellationToken).ConfigureAwait(false);
            if (!isValid)
            {
                return (null, null, new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = validationError }
                    ]
                });
            }

            var originalFileName = file.FileName;
            var storedFileName = $"{Guid.NewGuid()}.webp";
            var relativePath = Path.Combine("uploads", storedFileName);

            long fileSize;
            try
            {
                using var image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken).ConfigureAwait(false);
                using var outputStream = new MemoryStream();

                await image.SaveAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);
                fileSize = outputStream.Length;

                await fileRepository.SaveFileAsync(outputStream, relativePath, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errMsg = $"Image processing failed: {ex.Message}";
                return (null, null, new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = errMsg }]
                });
            }

            var publicUrl = $"{baseUrl}/uploads/{storedFileName}";

            var mediaFile = new MediaFile
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = "image/webp",
                PublicUrl = publicUrl,
                FileSize = fileSize,
            };

            var uploadResponse = new UploadResponse
            {
                IsSuccess = true,
                FileName = storedFileName,
                Url = publicUrl
            };

            return (mediaFile, uploadResponse, null);
        }

        public async Task<(UploadResponse? Data, ErrorResponse? Error)> UploadSingleFileAsync(
            IFormFile file, string baseUrl, CancellationToken cancellationToken)
        {
            var (mediaFile, responseData, error) = await ProcessAndSaveFileToDiskAsync(file, baseUrl, cancellationToken).ConfigureAwait(false);

            if (error is not null || mediaFile is null || responseData is null)
            {
                return (null, error ?? new ErrorResponse { Errors = [new ErrorDetail { Message = "Unknown error occurred." }] });
            }

            try
            {
                await mediaFileRepository.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
                await mediaFileRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return (null, new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Database save failed: {ex.Message}" }]
                });
            }

            return (responseData, null);
        }

        public async Task<(List<UploadResponse>? Data, ErrorResponse? Error)> UploadMultipleFilesAsync(
            List<IFormFile> files, string baseUrl, CancellationToken cancellationToken)
        {
            var tasks = files.Select(file => ProcessAndSaveFileToDiskAsync(file, baseUrl, cancellationToken));

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            var errorDetails = results.Where(r => r.Error is not null).SelectMany(r => r.Error!.Errors).ToList();
            if (errorDetails.Count != 0)
            {
                return (null, new ErrorResponse { Errors = errorDetails });
            }

            var data = new List<UploadResponse>();
            var mediaFilesToAdd = new List<MediaFile>();

            foreach (var (FileData, Response, _) in results)
            {
                if (FileData is not null && Response is not null)
                {
                    mediaFilesToAdd.Add(FileData);
                    data.Add(Response);
                }
            }

            if (mediaFilesToAdd.Count > 0)
            {
                try
                {
                    foreach (var mediaFile in mediaFilesToAdd)
                    {
                        await mediaFileRepository.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
                    }

                    await mediaFileRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return (null, new ErrorResponse
                    {
                        Errors = [new ErrorDetail { Message = $"Database save failed: {ex.Message}" }]
                    });
                }
            }

            return (data, null);
        }

        public async Task<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken)
        {
            var originalRelativePath = Path.Combine("uploads", fileName);
            if (!fileRepository.FileExists(originalRelativePath))
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found." }] });
            }

            if (width == null)
            {
                var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                return ((originalStream!, "image/webp"), null);
            }

            try
            {
                using var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                if (originalStream == null) return (null, null);

                using var image = await Image.LoadAsync(originalStream, cancellationToken).ConfigureAwait(false);

                var newWidth = width.Value;
                var newHeight = (int)((double)image.Height / image.Width * newWidth);

                image.Mutate(x => x.Resize(newWidth, newHeight));

                var outputStream = new MemoryStream();
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

                outputStream.Position = 0;
                return ((outputStream, "image/webp"), null);
            }
            catch (Exception)
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Image processing failed." }] });
            }
        }

        private async Task<(bool, string)> ValidateFileAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty.");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                return (false, "File size exceeds 10MB limit.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                return (false, "Invalid file type. Only JPG, PNG, GIF are allowed.");
            }

            if (!_mimeTypeMappings.ContainsValue(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid MIME type.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var info = await Image.IdentifyAsync(stream, cancellationToken).ConfigureAwait(false);
                if (info == null)
                {
                    return (false, "File is not a valid image.");
                }
            }
            catch
            {
                return (false, "File is corrupted or not a valid image.");
            }

            return (true, string.Empty);
        }

        public async Task<ErrorResponse?> DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] };
            }
            try
            {
                var media = await mediaFileRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);

                if (media is null)
                {
                    var existed = await mediaFileRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                    if (existed is not null)
                    {
                        return new ErrorResponse { Errors = [new ErrorDetail { Message = "File already deleted." }] };
                    }

                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] };
                }

                await mediaFileRepository.DeleteAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
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
                    var media = await mediaFileRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                    if (media is null)
                    {
                        var existed = await mediaFileRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                        if (existed is not null)
                        {
                            notFound.Add(f);
                            continue;
                        }

                        notFound.Add(f);
                        continue;
                    }

                    await mediaFileRepository.DeleteAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
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

        public async Task<ErrorResponse?> RestoreFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] };
            }

            try
            {
                var media = await mediaFileRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                if (media is null)
                {
                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] };
                }

                var checkActive = await mediaFileRepository.GetByStoredFileNameAsync(fileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                if (checkActive is not null)
                {
                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File is not deleted." }] };
                }

                await mediaFileRepository.RestoreAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
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
                    var media = await mediaFileRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                    if (media is null)
                    {
                        notFound.Add(f);
                        continue;
                    }

                    var isActive = await mediaFileRepository.GetByStoredFileNameAsync(f, cancellationToken, includeDeleted: false).ConfigureAwait(false);
                    if (isActive is not null)
                    {
                        notFound.Add(f);
                        continue;
                    }

                    await mediaFileRepository.RestoreAndSaveAsync(media, cancellationToken).ConfigureAwait(false);
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
