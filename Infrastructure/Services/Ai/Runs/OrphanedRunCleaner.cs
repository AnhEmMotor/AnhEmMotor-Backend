using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Ai.Runs;

public class OrphanedRunCleaner(
    IServiceProvider serviceProvider,
    ILogger<OrphanedRunCleaner> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Startup: mọi ChatRun status=Running/Pending -> Orphaned, lưu PartialOutput thành ChatMessage
        try
        {
            await ProcessStartupOrphansAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi dọn dẹp orphan runs lúc startup");
        }

        // 2. Mỗi 60s: run có HeartbeatAt cũ hơn 2 phút -> Orphaned
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                await ProcessTimeoutOrphansAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi quét timeout orphans");
            }
        }
    }

    private async Task ProcessStartupOrphansAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IChatRunWriter>();

        var orphans = await context.ChatRuns
            .Where(r => r.Status == ChatRunStatus.Running || r.Status == ChatRunStatus.Pending)
            .ToListAsync(stoppingToken);

        foreach (var run in orphans)
        {
            run.Status = ChatRunStatus.Orphaned;
            run.CompletedAt = DateTime.UtcNow;
            run.ErrorCode = "run_orphaned";

            if (!string.IsNullOrEmpty(run.PartialOutput))
            {
                var aiMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    SessionId = run.SessionId,
                    Role = ChatRole.Ai,
                    Message = run.PartialOutput,
                    CreatedAt = DateTime.UtcNow
                };
                context.ChatMessages.Add(aiMessage);
            }

            await writer.AppendAsync(run.Id, ChatRunEventType.Error, "run_orphaned");
        }

        if (orphans.Any())
        {
            await context.SaveChangesAsync(stoppingToken);
            logger.LogInformation("Đã dọn dẹp {Count} orphan runs lúc startup", orphans.Count);
        }
    }

    private async Task ProcessTimeoutOrphansAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IChatRunWriter>();

        var threshold = DateTime.UtcNow.AddMinutes(-2);
        
        var orphans = await context.ChatRuns
            .Where(r => r.Status == ChatRunStatus.Running && r.HeartbeatAt < threshold)
            .ToListAsync(stoppingToken);

        foreach (var run in orphans)
        {
            run.Status = ChatRunStatus.Orphaned;
            run.CompletedAt = DateTime.UtcNow;
            run.ErrorCode = "run_orphaned";

            if (!string.IsNullOrEmpty(run.PartialOutput))
            {
                var aiMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    SessionId = run.SessionId,
                    Role = ChatRole.Ai,
                    Message = run.PartialOutput,
                    CreatedAt = DateTime.UtcNow
                };
                context.ChatMessages.Add(aiMessage);
            }

            await writer.AppendAsync(run.Id, ChatRunEventType.Error, "run_orphaned");
        }

        if (orphans.Any())
        {
            await context.SaveChangesAsync(stoppingToken);
            logger.LogWarning("Đã phát hiện và dọn dẹp {Count} timeout orphans", orphans.Count);
        }
    }
}
