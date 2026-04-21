using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Banner
{
    public class BannerReadRepository(ApplicationDBContext context) : IBannerReadRepository
    {
        public async Task<Domain.Entities.Banner?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await context.Banners
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Domain.Entities.Banner>> GetActiveBannersAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            return await context.Banners
                .Where(b => b.IsActive && 
                           (!b.StartDate.HasValue || b.StartDate <= now) && 
                           (!b.EndDate.HasValue || b.EndDate >= now))
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
    }
}
