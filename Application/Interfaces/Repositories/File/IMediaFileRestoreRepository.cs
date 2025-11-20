using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileRestoreRepository
    {
        void Restore(MediaFile mediaFile);
        void Restores(List<MediaFile> mediaFiles);
    }
}
