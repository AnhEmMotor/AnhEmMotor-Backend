using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileDeleteRepository
    {
        Task DeleteAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken);
    }
}
