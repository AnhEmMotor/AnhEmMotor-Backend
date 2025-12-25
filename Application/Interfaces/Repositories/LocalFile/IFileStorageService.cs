namespace Application.Interfaces.Repositories.LocalFile;

public interface IFileStorageService
{
    public Task<(string StoragePath, string FileExtension)> SaveFileAsync(
        Stream file,
        CancellationToken cancellationToken);

    public Task<List<(string StoragePath, string FileExtension)>> SaveFilesAsync(
        IEnumerable<Stream> files,
        CancellationToken cancellationToken);

    public bool DeleteFile(string storagePath);

    public bool DeleteFile(IEnumerable<string> storagePaths);

    public string GetPublicUrl(string storagePath);

    public Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken);

    public Task<Stream> ReadImageAsync(Stream inputStream, int? width, CancellationToken cancellationToken);
}
