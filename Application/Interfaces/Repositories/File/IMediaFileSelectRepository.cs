using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileSelectRepository
    {
        /// <summary>
        /// Get media file by stored file name. By default only returns not-deleted records.
        /// Set includeDeleted = true to search including soft-deleted entries.
        /// </summary>
        ValueTask<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken, bool includeDeleted = false);
    }
}
