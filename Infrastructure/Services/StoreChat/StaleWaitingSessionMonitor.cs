using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.StoreChat;

public class StaleWaitingSessionMonitor(
    IServiceProvider serviceProvider,
    ILogger<StaleWaitingSessionMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                await CheckStaleWaitingSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi quét phiên Store Chat chờ nhân viên quá lâu");
            }
        }
    }

    // ponytail: LastMessageAt là proxy gần đúng cho thời điểm vào hàng đợi (entity chưa có cột
    // HandoffRequestedAt riêng) — thêm cột riêng nếu số liệu thực tế cho thấy lệch đáng kể.
    private async Task CheckStaleWaitingSessionsAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var threshold = DateTime.UtcNow.AddMinutes(-10);
        var stale = await context.StoreChatSessions
            .Where(s => s.Mode == StoreChatMode.Waiting && s.LastMessageAt < threshold)
            .ToListAsync(stoppingToken);
    }
}
