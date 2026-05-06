using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerAuditRepository
    {
        public void AddLog(BannerAuditLog log);
        public Task<List<BannerAuditLog>> GetLogsByBannerIdAsync(int bannerId, CancellationToken cancellationToken);
    }
}
