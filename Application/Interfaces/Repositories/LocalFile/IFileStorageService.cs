using Application.Common.Models;
using Application.Interfaces.Repositories.LocalFile;

namespace Application.Interfaces.Repositories.LocalFile;

public interface IFileStorageService
{
    public Task<Result<FileUpload>> SaveFileAsync(Stream file, CancellationToken cancellationToken);
    public bool DeleteFile(string storagePath);
    public bool DeleteFile(IEnumerable<string> storagePaths);
    public string GetPublicUrl(string storagePath);
    public Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(string storagePath, CancellationToken cancellationToken);
    public Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken);
    public Task<Stream> CompressImageAsync(Stream inputStream, int quality, int? maxWidth, CancellationToken cancellationToken);
}