using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.File
{
    public class MediaFileRepository(ApplicationDBContext context) : IMediaFileRepository
    {
        public async Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken)
        {
            await context.MediaFiles.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<int>(context.SaveChangesAsync(cancellationToken));
        }

        public ValueTask<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken)
        {
            var task = context.MediaFiles.FirstOrDefaultAsync(m => m.StoredFileName == storedFileName, cancellationToken);
            return new ValueTask<MediaFile?>(task);
        }

        public async Task DeleteAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken)
        {
            context.MediaFiles.Remove(mediaFile);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
