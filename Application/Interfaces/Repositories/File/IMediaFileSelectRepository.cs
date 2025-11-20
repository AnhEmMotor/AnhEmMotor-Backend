using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileSelectRepository
    {
        Task<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken, bool includeDeleted = false);
        Task<List<MediaFile>?> GetByStoredFileNamesAsync(List<string?> storedFileNames, CancellationToken cancellationToken, bool includeDeleted = false);
    }
}
