using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File;

public class MediaFileRestoreRepository(ApplicationDBContext context) : IMediaFileRestoreRepository
{
    public void Restore(MediaFile mediaFile)
    {
        context.Restore(mediaFile);
    }

    public void Restores(List<MediaFile> mediaFiles)
    {
        foreach (var media in mediaFiles)
        {
            context.Restore(media);
        }
    }
}