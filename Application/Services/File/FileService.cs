using Application.ApiContracts.File;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Entities;
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

        public async Task<UploadResponse> UploadSingleFileAsync(IFormFile file, string baseUrl, CancellationToken cancellationToken)
        {
            var (isValid, validationError) = await ValidateFileAsync(file, cancellationToken).ConfigureAwait(false);
            if (!isValid)
            {
                return new UploadResponse { IsSuccess = false, Error = validationError };
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
                return new UploadResponse { IsSuccess = false, Error = $"Image processing failed: {ex.Message}" };
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

            await mediaFileRepository.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
            await mediaFileRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new UploadResponse
            {
                IsSuccess = true,
                FileName = storedFileName,
                Url = publicUrl
            };
        }

        public async Task<List<UploadResponse>> UploadMultipleFilesAsync(List<IFormFile> files, string baseUrl, CancellationToken cancellationToken)
        {
            var tasks = files.Select(file => UploadSingleFileAsync(file, baseUrl, cancellationToken));
            var whenAllTask = Task.WhenAll(tasks);
            var results = await whenAllTask.ConfigureAwait(false);
            return [.. results];
        }

        public async Task<(Stream fileStream, string contentType)?> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken)
        {
            var originalRelativePath = Path.Combine("uploads", fileName);
            if (!fileRepository.FileExists(originalRelativePath))
            {
                return null;
            }

            if (width == null)
            {
                var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                return (originalStream!, "image/webp");
            }

            var cachedFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_w{width}.webp";
            var cachedRelativePath = Path.Combine("uploads", "cache", cachedFileName);

            if (fileRepository.FileExists(cachedRelativePath))
            {
                var cachedStream = await fileRepository.ReadFileAsync(cachedRelativePath, cancellationToken).ConfigureAwait(false);
                return (cachedStream!, "image/webp");
            }

            try
            {
                using var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                if (originalStream == null) return null;

                using var image = await Image.LoadAsync(originalStream, cancellationToken).ConfigureAwait(false);

                var newWidth = width.Value;
                var newHeight = (int)((double)image.Height / image.Width * newWidth);

                image.Mutate(x => x.Resize(newWidth, newHeight));

                var outputStream = new MemoryStream();
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

                await fileRepository.SaveFileAsync(outputStream, cachedRelativePath, cancellationToken).ConfigureAwait(false);

                outputStream.Position = 0;
                return (outputStream, "image/webp");
            }
            catch (Exception)
            {
                return null;
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
    }
}
