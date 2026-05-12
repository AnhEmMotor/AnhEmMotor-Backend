using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundJobs
{
    public class BannerExpiryWorker(IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateBannerStatusesAsync(stoppingToken).ConfigureAwait(false);
                } catch
                {
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateBannerStatusesAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var now = DateTimeOffset.UtcNow;
            var expiredBanners = await context.Banners
                .Where(b => b.IsActive && b.EndDate.HasValue && b.EndDate < now)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (expiredBanners.Any())
            {
                foreach (var banner in expiredBanners)
                {
                    banner.IsActive = false;
                    context.BannerAuditLogs
                        .Add(
                            new BannerAuditLog
                            {
                                BannerId = banner.Id,
                                Action = "Expire",
                                ChangedBy = "System (Background Worker)",
                                Details = $"Banner '{banner.Title}' automatically expired."
                            });
                }
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
