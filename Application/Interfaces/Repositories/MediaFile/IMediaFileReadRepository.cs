using Domain.Enums;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileReadRepository
{
    IQueryable<MediaFileEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<MediaFileEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<MediaFileEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<MediaFileEntity?> GetByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<MediaFileEntity>> GetByStoragePathsAsync(
        IEnumerable<string> storagePaths,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<MediaFileEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
