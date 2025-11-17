using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File
{
    public class MediaFileRestoreRepository(ApplicationDBContext context) : IMediaFileRestoreRepository
    {
        public async Task RestoreAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken)
        {
            context.Restore(mediaFile);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
