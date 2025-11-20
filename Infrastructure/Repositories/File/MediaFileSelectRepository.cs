using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.File;

public class MediaFileSelectRepository(ApplicationDBContext context) : IMediaFileSelectRepository
{
    public Task<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        var query = includeDeleted ? context.All<MediaFile>() : context.MediaFiles.AsQueryable();

        return query.FirstOrDefaultAsync(m => string.Compare(m.StoredFileName, storedFileName) == 0, cancellationToken);
    }

    public async Task<List<MediaFile>?> GetByStoredFileNamesAsync(List<string?> storedFileNames, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        if (storedFileNames == null || storedFileNames.Count == 0)
        {
            return [];
        }
        var query = includeDeleted ? context.All<MediaFile>() : context.MediaFiles.AsQueryable();
        var result = await query.Where(m => storedFileNames.Contains(m.StoredFileName)).ToListAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

}
