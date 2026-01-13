using Application.Common.Models;
using Application.Interfaces.Repositories.LocalFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Repositories.LocalFile;

public class LocalFileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor) : IFileStorageService
{
    private const int DefaultMaxWidth = 1200;
    private readonly string _uploadFolder = Path.Combine(environment.WebRootPath, "uploads");

    private readonly List<string> _allowedMimeTypes = [ "image/jpeg", "image/png", "image/gif", "image/webp" ];

    public async Task<Result<FileUpload>> SaveFileAsync(Stream file, CancellationToken cancellationToken)
    {
        try
        {
            if(file == null || file.Length == 0)
            {
                return Result<FileUpload>.Failure("File stream is empty");
            }

            if(!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }

            if(file.CanSeek)
            {
                file.Position = 0;
            }

            var format = await Image.DetectFormatAsync(file, cancellationToken).ConfigureAwait(false);

            if(format == null)
            {
                return Result<FileUpload>.Failure("Unable to detect image format");
            }

            if(!_allowedMimeTypes.Contains(format.DefaultMimeType))
            {
                return Result<FileUpload>.Failure($"Format {format.DefaultMimeType} is not supported");
            }

            var storageFileName = $"{Guid.NewGuid()}.webp";
            var fullPath = Path.Combine(_uploadFolder, storageFileName);

            using var compressedStream = await CompressImageAsync(file, 75, DefaultMaxWidth, cancellationToken)
                .ConfigureAwait(false);
            var compressedSize = compressedStream.Length;

            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await compressedStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

            return new FileUpload(storageFileName, ".webp", compressedSize);
        } catch(Exception ex)
        {
            return Result<FileUpload>.Failure(ex.Message);
        }
    }

    public bool DeleteFile(string storagePath)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if(File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                return true;
            } catch
            {
                return false;
            }
        }
        return false;
    }

    public bool DeleteFile(IEnumerable<string> storagePaths)
    {
        var allDeleted = true;
        foreach(var path in storagePaths)
            if(!DeleteFile(path))
                allDeleted = false;
        return allDeleted;
    }

    public string GetPublicUrl(string storagePath)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if(request == null)
            return $"/api/v1/mediafile/view-image/{storagePath}";
        return $"{request.Scheme}://{request.Host.Value}/api/v1/mediafile/view-image/{storagePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if(!File.Exists(fullPath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        return (fileBytes, "image/webp");
    }

    public async Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken)
    {
        var result = await CompressImageAsync(inputStream, 75, width, cancellationToken).ConfigureAwait(false);
        return result;

    }

    public async Task<Stream> CompressImageAsync(
        Stream inputStream,
        int quality,
        int? maxWidth,
        CancellationToken cancellationToken)
    {
        if(inputStream.CanSeek)
            inputStream.Position = 0;

        using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);

        var targetWidth = maxWidth ?? DefaultMaxWidth;

        if(image.Width > targetWidth)
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