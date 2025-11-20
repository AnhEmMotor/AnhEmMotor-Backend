using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File;

public class MediaFileInsertRepository(ApplicationDBContext context) : IMediaFileInsertRepository
{
    public async Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken)
    {
        await context.MediaFiles.AddAsync(mediaFile, cancellationToken).ConfigureAwait(false);
    }
}