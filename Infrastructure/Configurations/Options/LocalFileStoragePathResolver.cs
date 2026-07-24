using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Configurations.Options;

public static class LocalFileStoragePathResolver
{
    public static string Resolve(
        IWebHostEnvironment environment,
        IOptions<LocalFileStorageOptions> options)
    {
        var configuredPath = options.Value.UploadPath?.Trim();
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var absolutePath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);
            return Path.GetFullPath(absolutePath);
        }

        var webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        return Path.GetFullPath(Path.Combine(webRootPath, "uploads"));
    }
}
