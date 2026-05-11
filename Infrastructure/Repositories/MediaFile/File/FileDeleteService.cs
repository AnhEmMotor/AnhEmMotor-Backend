using Application.Interfaces.Repositories.MediaFile.File;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.MediaFile.File;

public class FileDeleteService : IFileDeleteService
{
    private readonly string _uploadFolder;

    public FileDeleteService(
        IWebHostEnvironment environment,
        IOptions<LocalFileStorageOptions> options)
    {
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

    public bool DeleteFile(string storagePath)
    {
        var fullPath = Path.Combine(_uploadFolder, storagePath);
        if (System.IO.File.Exists(fullPath))
        {
            try
            {
                System.IO.File.Delete(fullPath);
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
        foreach (var path in storagePaths)
            if (!DeleteFile(path))
                allDeleted = false;
        return allDeleted;
    }
}
