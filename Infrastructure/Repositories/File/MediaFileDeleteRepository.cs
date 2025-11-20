using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File;

public class MediaFileDeleteRepository(ApplicationDBContext context) : IMediaFileDeleteRepository
{
    public void Delete(MediaFile mediaFile)
    {
        context.MediaFiles.Remove(mediaFile);
    }

    public void DeleteRange(IEnumerable<MediaFile> mediaFiles)
    {
        context.MediaFiles.RemoveRange(mediaFiles);
    }
}