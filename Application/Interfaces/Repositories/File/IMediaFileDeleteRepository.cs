using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileDeleteRepository
    {
        void Delete(MediaFile mediaFile);
        void DeleteRange(IEnumerable<MediaFile> mediaFiles);
    }
}
