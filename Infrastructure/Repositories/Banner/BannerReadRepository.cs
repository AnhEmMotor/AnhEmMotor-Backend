using Application.Interfaces.Repositories.Banner;
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
            var now = DateTimeOffset.UtcNow;
            return context.Banners
                .Where(
                    b => b.IsActive &&
                        (!b.StartDate.HasValue || b.StartDate <= now) &&
                        (!b.EndDate.HasValue || b.EndDate >= now))
                .OrderByDescending(b => b.Priority)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.Banner>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return context.Banners.OrderByDescending(b => b.Priority).ToListAsync(cancellationToken);
        }
    }
}
