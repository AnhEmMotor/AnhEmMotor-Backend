namespace Application.Interfaces.Repositories.LocalFile;

public interface IFileStorageService
{
    Task<(string StoragePath, string FileExtension)> SaveFileAsync(Stream file, CancellationToken cancellationToken);

    Task<List<(string StoragePath, string FileExtension)>> SaveFilesAsync(
        IEnumerable<Stream> files,
        CancellationToken cancellationToken);

    bool DeleteFile(string storagePath);

    bool DeleteFile(IEnumerable<string> storagePaths);

    string GetPublicUrl(string storagePath);

    Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(string storagePath, CancellationToken cancellationToken);

    Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken);
}
