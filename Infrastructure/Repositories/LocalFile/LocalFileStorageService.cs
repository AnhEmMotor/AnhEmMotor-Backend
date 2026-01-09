using Application.Interfaces.Repositories.LocalFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Repositories.LocalFile;

public class LocalFileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : IFileStorageService
{
    private const int DefaultMaxWidth = 1200;
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly string _uploadFolder = Path.Combine(environment.WebRootPath, "uploads");

    private readonly List<string> _allowedMimeTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public async Task<(string StoragePath, string FileExtension)> SaveFileAsync(
        Stream file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 10MB limit.");

        if (!Directory.Exists(_uploadFolder))
            Directory.CreateDirectory(_uploadFolder);

        IImageFormat format;
        try
        {
            if (file.CanSeek)
                file.Position = 0;

            format = await Image.DetectFormatAsync(file, cancellationToken).ConfigureAwait(false);

            if (format == null)
                throw new ArgumentException("File is not a recognized image format.");

            if (!_allowedMimeTypes.Contains(format.DefaultMimeType))
            {
                throw new ArgumentException(
                    $"File type {format.DefaultMimeType} is not allowed. Only JPG, PNG, GIF, WEBP.");
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("File is corrupted or not a valid image.");
        }

        var storagePath = $"{Guid.NewGuid()}.webp";
        var fullPath = Path.Combine(_uploadFolder, storagePath);

        try
        {
            using var compressedStream = await CompressImageAsync(file, 75, DefaultMaxWidth, cancellationToken).ConfigureAwait(false);
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await compressedStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

            return (storagePath, ".webp");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process image.", ex);
        }
    }

    public async Task<List<(string StoragePath, string FileExtension)>> SaveFilesAsync(
        IEnumerable<Stream> files,
        CancellationToken cancellationToken)
    {
        var results = new List<(string, string)>();
        foreach (var file in files)
        {
            results.Add(await SaveFileAsync(file, cancellationToken).ConfigureAwait(false));
        }
        return results;
    }

    public bool DeleteFile(string storagePath)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public bool DeleteFile(IEnumerable<string> storagePaths)
    {
        var allDeleted = true;
        foreach (var path in storagePaths)
            if (!DeleteFile(path))
                allDeleted = false;
        return allDeleted;
    }

    public string GetPublicUrl(string storagePath)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return $"/api/v1/mediafile/view-image/{storagePath}";
        return $"{request.Scheme}://{request.Host.Value}/api/v1/mediafile/view-image/{storagePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if (!File.Exists(fullPath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        return (fileBytes, "image/webp");
    }

    public async Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken)
    {
        return await CompressImageAsync(inputStream, 75, width, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> CompressImageAsync(Stream inputStream, int quality, int? maxWidth, CancellationToken cancellationToken)
    {
        if (inputStream.CanSeek)
            inputStream.Position = 0;

        using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);

        var targetWidth = maxWidth ?? DefaultMaxWidth;

        if (image.Width > targetWidth)
        {
            var newHeight = (int)((double)targetWidth / image.Width * image.Height);
            image.Mutate(x => x.Resize(targetWidth, newHeight));
        }

        var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = quality }, cancellationToken)
            .ConfigureAwait(false);

        outputStream.Position = 0;
        return outputStream;
    }
}