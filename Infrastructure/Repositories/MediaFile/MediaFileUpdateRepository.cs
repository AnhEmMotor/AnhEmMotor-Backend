using Application.Interfaces.Repositories.MediaFile;
using Infrastructure.DBContexts;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile;

public class MediaFileUpdateRepository(ApplicationDBContext context) : IMediaFileUpdateRepository
{
    public void Update(MediaFileEntity file)
    {
        context.MediaFiles.Update(file);
    }

    public void Restore(MediaFileEntity file)
    {
        context.Restore(file);
    }

    public void Restore(IEnumerable<MediaFileEntity> files)
    {
        foreach (var file in files)
        {
            context.Restore(file);
        }
    }
}
