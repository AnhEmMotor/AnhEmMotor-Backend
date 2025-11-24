using Application.Interfaces.Repositories.MediaFile;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile;

public class MediaFileReadRepository(ApplicationDBContext context) : IMediaFileReadRepository
{
    public async Task<IEnumerable<MediaFileEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<MediaFileEntity>(mode)
            .ToListAsync(cancellationToken);
    }

    public async Task<MediaFileEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<MediaFileEntity?> GetByStoragePathAsync(string storagePath, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => f.StoragePath == storagePath, cancellationToken);
    }

    public async Task<IEnumerable<MediaFileEntity>> GetByStoragePathsAsync(IEnumerable<string> storagePaths, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<MediaFileEntity>(mode)
            .Where(f => storagePaths.Contains(f.StoragePath))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MediaFileEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<MediaFileEntity>(mode)
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<MediaFileEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode);
    }
}
