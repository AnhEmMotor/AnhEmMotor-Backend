
using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerReadRepository
    {
        public Task<Domain.Entities.Banner?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<List<Domain.Entities.Banner>> GetBannersByPlacementAsync(CancellationToken cancellationToken, string? placement = null);

        public Task<List<Domain.Entities.Banner>> GetAllAsync(CancellationToken cancellationToken = default);

        public Task<List<BannerAuditLog>> GetLogsByBannerIdAsync(int bannerId, CancellationToken cancellationToken);
    }
}
