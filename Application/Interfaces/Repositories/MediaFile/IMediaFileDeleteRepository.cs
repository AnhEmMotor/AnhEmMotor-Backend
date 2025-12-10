using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileDeleteRepository
{
    public void Delete(MediaFileEntity file);

    public void Delete(IEnumerable<MediaFileEntity> files);
}
