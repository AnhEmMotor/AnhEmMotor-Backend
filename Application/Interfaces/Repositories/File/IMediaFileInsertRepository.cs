using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileInsertRepository
    {
        void Add(MediaFile mediaFile);
    }
}
