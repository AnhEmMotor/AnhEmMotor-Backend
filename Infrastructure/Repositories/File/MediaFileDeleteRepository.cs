using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File
{
    public class MediaFileDeleteRepository(ApplicationDBContext context) : IMediaFileDeleteRepository
    {
        public async Task DeleteAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken)
        {
            context.MediaFiles.Remove(mediaFile);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
