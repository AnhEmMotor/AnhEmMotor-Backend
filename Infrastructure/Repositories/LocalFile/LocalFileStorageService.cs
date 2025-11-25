using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Infrastructure.Repositories.LocalFile;

public class LocalFileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : Application.Interfaces.Repositories.LocalFile.IFileStorageService
{
    private const int DefaultMaxWidth = 1200;
    private readonly string _uploadFolder = Path.Combine(environment.WebRootPath, "uploads");
    private readonly string[] _permittedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private readonly Dictionary<string, string> _mimeTypeMappings = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" }
    };

    public async Task<(string StoragePath, string FileExtension)> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }

        await ValidateImageFileAsync(file, cancellationToken).ConfigureAwait(false);

        var storagePath = $"{Guid.NewGuid()}.webp";
        var fullPath = Path.Combine(_uploadFolder, storagePath);

        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);
        
        await image.SaveAsWebpAsync(fullPath, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

        return (storagePath, ".webp");
    }

    public async Task<List<(string StoragePath, string FileExtension)>> SaveFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
    {
        var results = new List<(string, string)>();

        foreach (var file in files)
        {
            var result = await SaveFileAsync(file, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    public Task<bool> DeleteFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task<bool> DeleteFilesAsync(IEnumerable<string> storagePaths, CancellationToken cancellationToken)
    {
        var allDeleted = true;

        foreach (var path in storagePaths)
        {
            var deleted = await DeleteFileAsync(path, cancellationToken).ConfigureAwait(false);
            if (!deleted) allDeleted = false;
        }

        return allDeleted;
    }

    public string GetPublicUrl(string storagePath)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return $"/api/v1/mediafile/view-image/{storagePath}";

        var scheme = request.Scheme;
        var host = request.Host.Value;
        return $"{scheme}://{host}/api/v1/mediafile/view-image/{storagePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        var contentType = GetContentType(Path.GetExtension(storagePath));

        return (fileBytes, contentType);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private async Task ValidateImageFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty.");
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB
        {
            throw new ArgumentException("File size exceeds 10MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_permittedImageExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid file type. Only JPG, PNG, GIF, WEBP are allowed.");
        }

        if (!_mimeTypeMappings.ContainsValue(file.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException("Invalid MIME type.");
        }

        // Validate that it's a real image
        try
        {
            using var stream = file.OpenReadStream();
            var imageInfo = await Image.IdentifyAsync(stream, cancellationToken).ConfigureAwait(false) ?? throw new ArgumentException("File is not a valid image.");
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new ArgumentException("File is corrupted or not a valid image.");
        }
    }

    public async Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);

        var targetWidth = width ?? DefaultMaxWidth;

        if (image.Width > targetWidth)
        {
            var newHeight = (int)((double)targetWidth / image.Width * image.Height);
            image.Mutate(x => x.Resize(targetWidth, newHeight));
        }

        var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

        outputStream.Position = 0;
        return outputStream;
    }
}
