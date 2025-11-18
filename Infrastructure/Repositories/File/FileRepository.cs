using Application.Interfaces.Repositories.File;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Repositories.File
{
    public class FileRepository(IWebHostEnvironment webHostEnvironment) : IFileRepository
    {
        private readonly string _WebRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(_WebRootPath, relativePath);
        }

        public bool FileExists(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            return System.IO.File.Exists(fullPath);
        }

        public async Task<Stream?> ReadFileAsync(string relativePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = GetFullPath(relativePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        }

        public async Task SaveFileAsync(Stream stream, string relativePath, CancellationToken cancellationToken)
        {
            var fullPath = GetFullPath(relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            stream.Position = 0;
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = GetFullPath(relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch
                {
                }
            }

            await Task.CompletedTask;
        }

        public async Task DeleteFilesByPrefixAsync(string directoryRelativePath, string prefix, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dirFullPath = GetFullPath(directoryRelativePath);
            if (!Directory.Exists(dirFullPath))
            {
                return;
            }

            try
            {
                var files = Directory.GetFiles(dirFullPath).Where(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                foreach (var f in files)
                {
                    try
                    {
                        System.IO.File.Delete(f);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            await Task.CompletedTask;
        }
    }
}
