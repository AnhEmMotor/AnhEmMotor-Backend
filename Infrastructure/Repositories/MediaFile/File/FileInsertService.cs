using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Infrastructure.Repositories.MediaFile.File;

public class FileInsertService : IFileInsertService
{
    private const int DefaultMaxWidth = 1200;
    private readonly string _uploadFolder;
    private readonly IFileUpdateService _fileUpdateService;
    private readonly List<string> _allowedMimeTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public FileInsertService(
        IWebHostEnvironment environment,
        IOptions<LocalFileStorageOptions> options,
        IFileUpdateService fileUpdateService)
    {
        _fileUpdateService = fileUpdateService;
        var configPath = options.Value.UploadPath;
        if (!string.IsNullOrEmpty(configPath))
        {
            _uploadFolder = configPath;
        } else if (string.IsNullOrEmpty(environment.WebRootPath))
        {
            _uploadFolder = Path.Combine(Path.GetTempPath(), "AnhEmMotor_Uploads");
        } else
        {
            _uploadFolder = Path.Combine(environment.WebRootPath, "uploads");
        }
    }

    public async Task<Result<FileUpload>> SaveFileAsync(
        Stream file,
        CancellationToken cancellationToken,
        string subFolder = "")
    {
        try
        {
            if (file is null || file.Length is 0)
            {
                return Result<FileUpload>.Failure("File stream is empty");
            }
            var targetFolder = string.IsNullOrWhiteSpace(subFolder)
                ? _uploadFolder
                : Path.Combine(_uploadFolder, subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            if (file.CanSeek)
            {
                file.Position = 0;
            }
            var format = await Image.DetectFormatAsync(file, cancellationToken).ConfigureAwait(false);
            if (format is null)
            {
                return Result<FileUpload>.Failure("Unable to detect image format");
            }
            if (!_allowedMimeTypes.Contains(format.DefaultMimeType))
            {
                return Result<FileUpload>.Failure($"Format {format.DefaultMimeType} is not supported");
            }
            var storageFileName = $"{Guid.NewGuid()}.webp";
            var relativePath = string.IsNullOrWhiteSpace(subFolder)
                ? storageFileName
                : Path.Combine(subFolder, storageFileName).Replace("\\", "/");
            var fullPath = Path.Combine(_uploadFolder, relativePath);
            using var compressedStream = await _fileUpdateService.CompressImageAsync(
                file,
                75,
                DefaultMaxWidth,
                cancellationToken)
                .ConfigureAwait(false);
            var compressedSize = compressedStream.Length;
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await compressedStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            return new FileUpload(relativePath, ".webp", compressedSize);
        } catch (Exception ex)
        {
            return Result<FileUpload>.Failure(ex.Message);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters for Windows filenames
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        // Replace spaces with underscores for cleaner URLs
        sanitized = sanitized.Replace(' ', '_');
        // Ensure we have a valid filename
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "file";
        }
        return sanitized;
    }

    public async Task<Result<FileUpload>> SaveFileAsIsAsync(
        Stream file,
        string fileName,
        CancellationToken cancellationToken,
        string subFolder = "")
    {
        try
        {
            if (file is null || file.Length is 0)
            {
                return Result<FileUpload>.Failure("File stream is empty");
            }
            var targetFolder = string.IsNullOrWhiteSpace(subFolder)
                ? _uploadFolder
                : Path.Combine(_uploadFolder, subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            if (file.CanSeek)
            {
                file.Position = 0;
            }
            var extension = Path.GetExtension(fileName).ToLower();
            var originalNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var sanitizedName = SanitizeFileName(originalNameWithoutExt);
            var storageFileName = $"{sanitizedName}_{Guid.NewGuid()}{extension}";
            var relativePath = string.IsNullOrWhiteSpace(subFolder)
                ? storageFileName
                : Path.Combine(subFolder, storageFileName).Replace("\\", "/");
            var fullPath = Path.Combine(_uploadFolder, relativePath);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
            var size = file.Length;
            return new FileUpload(relativePath, extension, size);
        }
        catch (Exception ex)
        {
            return Result<FileUpload>.Failure(ex.Message);
        }
    }
}
