using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunEventCleanupJob(
    IServiceProvider serviceProvider,
    ILogger<ChatRunEventCleanupJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessCleanupAsync(stoppingToken);
                
                // Chạy mỗi 24h
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi chạy ChatRunEventCleanupJob");
                
                // Thử lại sau 1 giờ nếu lỗi
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task ProcessCleanupAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var threshold = DateTime.UtcNow.AddDays(-7);

        var deletedRows = await context.ChatRunEvents
            .Where(e => e.CreatedAt < threshold)
            .ExecuteDeleteAsync(stoppingToken);

        if (deletedRows > 0)
        {
            logger.LogInformation("Đã dọn dẹp {Count} ChatRunEvents cũ hơn 7 ngày", deletedRows);
        }
    }
}
