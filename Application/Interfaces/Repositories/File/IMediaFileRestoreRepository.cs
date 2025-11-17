using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileRestoreRepository
    {
        Task RestoreAndSaveAsync(MediaFile mediaFile, CancellationToken cancellationToken);
    }
}
