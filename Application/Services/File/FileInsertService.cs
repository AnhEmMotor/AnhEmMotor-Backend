using Application.ApiContracts.File;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Application.Services.File
{
    public class FileInsertService(IFileRepository fileRepository, IMediaFileInsertRepository mediaFileInsertRepository) : IFileInsertService
    {
        private readonly string[] _PermittedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
        private readonly Dictionary<string, string> _MimeTypeMappings = new()
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
                await mediaFileInsertRepository.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
                await mediaFileInsertRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
                        await mediaFileInsertRepository.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
                    }

                    await mediaFileInsertRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(ext) || !_PermittedExtensions.Contains(ext))
            {
                return (false, "Invalid file type. Only JPG, PNG, GIF are allowed.");
            }

            if (!_MimeTypeMappings.ContainsValue(file.ContentType.ToLowerInvariant()))
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
    }
}
