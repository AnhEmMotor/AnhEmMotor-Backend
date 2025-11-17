using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileRepository
    {
        Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken);
        ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken);
        ValueTask<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken, bool includeDeleted = false);
        Task DeleteAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken);
        Task RestoreAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken);
    }
}
