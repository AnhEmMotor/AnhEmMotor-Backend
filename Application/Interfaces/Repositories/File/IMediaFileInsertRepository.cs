using Domain.Entities;

namespace Application.Interfaces.Repositories.File
{
    public interface IMediaFileInsertRepository
    {
        Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken);
    }
}
