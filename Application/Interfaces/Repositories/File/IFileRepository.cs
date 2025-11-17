namespace Application.Interfaces.Repositories.File
{
    public interface IFileRepository
    {
        Task SaveFileAsync(Stream stream, string relativePath, CancellationToken cancellationToken);
        Task<Stream?> ReadFileAsync(string relativePath, CancellationToken cancellationToken);
        bool FileExists(string relativePath);
        Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken);
        Task DeleteFilesByPrefixAsync(string directoryRelativePath, string prefix, CancellationToken cancellationToken);
    }
}
