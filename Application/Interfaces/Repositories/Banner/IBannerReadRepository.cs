using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerReadRepository
    {
        Task<Domain.Entities.Banner?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<List<Domain.Entities.Banner>> GetActiveBannersAsync(CancellationToken cancellationToken);
    }
}
