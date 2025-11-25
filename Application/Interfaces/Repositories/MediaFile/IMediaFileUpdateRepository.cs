using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileUpdateRepository
{
    void Update(MediaFileEntity file);

    void Restore(MediaFileEntity file);

    void Restore(IEnumerable<MediaFileEntity> files);
}
