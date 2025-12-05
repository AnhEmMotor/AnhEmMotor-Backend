using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile;

public class MediaFileReadRepository(ApplicationDBContext context) : IMediaFileReadRepository
{
    public Task<IEnumerable<MediaFileEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
    }

    public Task<MediaFileEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<MediaFileEntity?> GetByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => string.Compare(f.StoragePath, storagePath) == 0, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
        ;
    }

    public Task<IEnumerable<MediaFileEntity>> GetByStoragePathsAsync(
        IEnumerable<string> storagePaths,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .Where(f => storagePaths.Contains(f.StoragePath))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
        ;
    }

    public Task<IEnumerable<MediaFileEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
        ;
    }

    public IQueryable<MediaFileEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<MediaFileEntity>(mode); }
}
