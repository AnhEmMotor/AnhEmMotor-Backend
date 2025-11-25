using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileDeleteRepository
{
    void Delete(MediaFileEntity file);

    void Delete(IEnumerable<MediaFileEntity> files);
}
