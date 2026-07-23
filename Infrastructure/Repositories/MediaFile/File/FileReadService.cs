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
        storagePath = storagePath.TrimStart('/', '\\');
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return $"/api/v1/MediaFile/view-image/{storagePath}";
        return $"{request.Scheme}://{request.Host.Value}/api/v1/MediaFile/view-image/{storagePath}";
    }

    public async Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        storagePath = storagePath.TrimStart('/', '\\');
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        var env = _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        var webRootPath = env != null && !string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(env.WebRootPath, storagePath)
            : string.Empty;
        if (!System.IO.File.Exists(fullPath))
        {
            if (System.IO.File.Exists(webRootPath))
            {
                fullPath = webRootPath;
            } else
            {
                var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", storagePath);
                if (System.IO.File.Exists(fallbackPath))
                {
                    fullPath = fallbackPath;
                } else
                {
                    var uploadsFallbackPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        storagePath);
                    if (System.IO.File.Exists(uploadsFallbackPath))
                    {
                        fullPath = uploadsFallbackPath;
                    }
                }
            }
        }
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
