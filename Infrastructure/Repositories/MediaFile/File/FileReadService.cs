using Application.Interfaces.Repositories.MediaFile.File;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.MediaFile.File;

public class FileReadService : IFileReadService
{
    private readonly string _uploadFolder;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileUpdateService _fileUpdateService;

    public FileReadService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IOptions<LocalFileStorageOptions> options,
        IFileUpdateService fileUpdateService)
    {
        _httpContextAccessor = httpContextAccessor;
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

    public string GetPublicUrl(string storagePath)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return $"/api/v1/MediaFile/view-image/{storagePath}";
        return $"{request.Scheme}://{request.Host.Value}/api/v1/MediaFile/view-image/{storagePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if (!System.IO.File.Exists(fullPath))
            return null;
        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        return (fileBytes, "image/webp");
    }

    public Task<Stream> ReadImageAsync(Stream InventoryReceiptStream, int? width, CancellationToken cancellationToken)
    {
        return _fileUpdateService.CompressImageAsync(InventoryReceiptStream, 75, width, cancellationToken);
    }
}
