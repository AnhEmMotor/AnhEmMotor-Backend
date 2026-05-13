using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Infrastructure.DBContexts;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile.MediaFile;

public class MediaFileInsertRepository(ApplicationDBContext context) : IMediaFileInsertRepository
{
    public void Add(MediaFileEntity file)
    {
        context.MediaFiles.Add(file);
    }

    public void AddRange(IEnumerable<MediaFileEntity> files)
    {
        context.MediaFiles.AddRange(files);
    }
}
