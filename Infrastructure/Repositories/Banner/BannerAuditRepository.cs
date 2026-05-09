using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Banner
{
    public class BannerAuditRepository(ApplicationDBContext context) : IBannerAuditRepository
    {
        public void AddLog(BannerAuditLog log)
        {
            context.BannerAuditLogs.Add(log);
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
