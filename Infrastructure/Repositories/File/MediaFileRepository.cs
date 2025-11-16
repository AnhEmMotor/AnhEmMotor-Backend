using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

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
    }
}
