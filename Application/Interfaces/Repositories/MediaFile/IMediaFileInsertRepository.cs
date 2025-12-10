using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileInsertRepository
{
    public void Add(MediaFileEntity file);

    public void AddRange(IEnumerable<MediaFileEntity> files);
}
