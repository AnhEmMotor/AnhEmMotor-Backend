using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Infrastructure.DBContexts;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile.MediaFile;

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
