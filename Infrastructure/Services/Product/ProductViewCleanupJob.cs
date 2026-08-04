using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Product;

public class ProductViewCleanupJob(IServiceProvider serviceProvider, ILogger<ProductViewCleanupJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessCleanupAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            } catch (OperationCanceledException)
            {
                break;
            } catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi chạy ProductViewCleanupJob");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task ProcessCleanupAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var threshold = DateTimeOffset.UtcNow.AddDays(-90);
        var deletedRows = await context.ProductViews
            .Where(v => v.CreatedAt < threshold)
            .ExecuteDeleteAsync(stoppingToken);
        if (deletedRows > 0)
        {
            logger.LogInformation("Đã dọn dẹp {Count} ProductViews cũ hơn 90 ngày", deletedRows);
        }
    }
}
