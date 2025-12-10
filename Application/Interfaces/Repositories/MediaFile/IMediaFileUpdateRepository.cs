using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Interfaces.Repositories.MediaFile;

public interface IMediaFileUpdateRepository
{
    public void Update(MediaFileEntity file);

    public void Restore(MediaFileEntity file);

    public void Restore(IEnumerable<MediaFileEntity> files);
}
