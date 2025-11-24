using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<(string StoragePath, string FileExtension)> SaveFileAsync(IFormFile file, CancellationToken cancellationToken);
    Task<List<(string StoragePath, string FileExtension)>> SaveFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken);
    Task<bool> DeleteFileAsync(string storagePath, CancellationToken cancellationToken);
    Task<bool> DeleteFilesAsync(IEnumerable<string> storagePaths, CancellationToken cancellationToken);
    string GetPublicUrl(string storagePath);
    Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(string storagePath, CancellationToken cancellationToken);
}
