using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs
{
    public class BannerExpiryWorker(IServiceProvider serviceProvider, ILogger<BannerExpiryWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Banner Expiry Worker is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateBannerStatuses(stoppingToken);
                } catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while updating banner statuses.");
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            logger.LogInformation("Banner Expiry Worker is stopping.");
        }

        private async Task UpdateBannerStatuses(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var now = DateTimeOffset.UtcNow;
            var expiredBanners = await context.Banners
                .Where(b => b.IsActive && b.EndDate.HasValue && b.EndDate < now)
                .ToListAsync(cancellationToken);
            if (expiredBanners.Any())
            {
                logger.LogInformation(
                    "Found {Count} expired banners. Updating status to inactive.",
                    expiredBanners.Count);
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
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
