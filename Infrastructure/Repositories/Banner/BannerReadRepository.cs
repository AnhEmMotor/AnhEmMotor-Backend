using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Banner
{
    public class BannerReadRepository(ApplicationDBContext context) : IBannerReadRepository
    {
        public Task<Domain.Entities.Banner?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.Banners.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public Task<List<Domain.Entities.Banner>> GetActiveBannersAsync(CancellationToken cancellationToken)
        {
            return context.Banners
                .ToListAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.Banner>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return context.Banners.ToListAsync(cancellationToken);
        }

        public Task<List<BannerAuditLog>> GetLogsByBannerIdAsync(int bannerId, CancellationToken cancellationToken)
        {
            return context.BannerAuditLogs
                .Where(l => l.BannerId == bannerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
