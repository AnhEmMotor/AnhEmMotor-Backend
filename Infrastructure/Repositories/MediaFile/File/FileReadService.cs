using Application.Interfaces.Repositories.MediaFile.File;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.MediaFile.File;

public class FileReadService : IFileReadService
{
    private readonly string _uploadFolder;
    private readonly IFileUpdateService _fileUpdateService;

    public FileReadService(
        IWebHostEnvironment environment,
        IOptions<LocalFileStorageOptions> options,
        IFileUpdateService fileUpdateService)
    {
        _fileUpdateService = fileUpdateService;
        _uploadFolder = LocalFileStoragePathResolver.Resolve(environment, options);
    }

    public string GetPublicUrl(string storagePath)
    {
        var normalizedStoragePath = storagePath.Replace('\\', '/').TrimStart('/');
        return $"/api/v1/MediaFile/view-image/{normalizedStoragePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if (!System.IO.File.Exists(fullPath))
            return null;
        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(storagePath).ToLower();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
        return (fileBytes, contentType);
    }

    public Task<Stream> ReadImageAsync(Stream InventoryReceiptStream, int? width, CancellationToken cancellationToken)
    {
        return _fileUpdateService.CompressImageAsync(InventoryReceiptStream, 75, width, cancellationToken);
    }
}
