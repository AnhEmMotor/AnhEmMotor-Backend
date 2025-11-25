using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileInsertRepository
{
    void Add(MediaFileEntity file);

    void AddRange(IEnumerable<MediaFileEntity> files);
}
