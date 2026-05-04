using Application.Interfaces.Repositories.MediaFile;
using Infrastructure.DBContexts;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile;

public class MediaFileDeleteRepository(ApplicationDBContext context) : IMediaFileDeleteRepository
{
    public void Delete(MediaFileEntity file)
    {
        context.MediaFiles.Remove(file);
    }

    public void Delete(IEnumerable<MediaFileEntity> files)
    {
        context.MediaFiles.RemoveRange(files);
    }
}
