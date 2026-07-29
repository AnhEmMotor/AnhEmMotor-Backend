using System.Collections.Concurrent;
using System.Text;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunExecutor(
    IChatRunQueue queue,
    IServiceProvider serviceProvider,
    IChatRunCancellationRegistry cancellationRegistry,
    ILogger<ChatRunExecutor> logger) : BackgroundService
{
    // Batching theo đúng thiết kế 08-STAGE-RUN-ENGINE.md mục "Cân nhắc hiệu năng": flush mỗi
    // 100ms HOẶC 100 ký tự, tuỳ điều kiện nào tới trước — thiếu điều kiện ký tự sẽ khiến 1 lần
    // flush gom quá nhiều chữ nếu sidecar đẩy dữ liệu nhanh, làm FE thấy "nhảy cục" thay vì mượt.
    private const int FlushIntervalMs = 100;
    private const int FlushCharThreshold = 100;
    private readonly SemaphoreSlim _semaphore = new(10); // 10 concurrent runs
    private readonly ConcurrentDictionary<Guid, Task> _inFlightRuns = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var runId in queue.ReadAllAsync(stoppingToken))
        {
            await _semaphore.WaitAsync(stoppingToken);

            var runTask = Task.Run(async () =>
            {
                try
                {
                    await ProcessRunAsync(runId, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lỗi khi xử lý run {RunId}", runId);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, stoppingToken);

            _inFlightRuns[runId] = runTask;
            _ = runTask.ContinueWith(
                _ => ((IDictionary<Guid, Task>)_inFlightRuns).Remove(runId),
                TaskScheduler.Default);
        }
    }

    // Host gọi StopAsync khi app shutdown (ApplicationStopping) — đợi các run đang chạy
    // huỷ + flush buffer (catch OperationCanceledException ở ProcessRunAsync) trước khi thoát,
    // tránh mất tối đa 200ms nội dung đang trong chunkBuffer.
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await Task.WhenAll(_inFlightRuns.Values);
    }

    private async Task ProcessRunAsync(Guid runId, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IChatRunWriter>();
        var streamClient = scope.ServiceProvider.GetRequiredService<ISidecarStreamClient>();
        var readRepo = scope.ServiceProvider.GetRequiredService<IChatReadRepository>();
        var tokenStore = scope.ServiceProvider.GetRequiredService<IChatRunTokenStore>();

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        runCts.CancelAfter(TimeSpan.FromMinutes(5));

        cancellationRegistry.Register(runId, runCts);

        // Khai báo ngoài try để catch (khi bị huỷ giữa chừng) vẫn lưu được phần đã sinh ra.
        var fullOutput = new StringBuilder();
        var chunkBuffer = new StringBuilder();
        var segmentStartedAt = DateTime.UtcNow;

        try
        {
            var run = await readRepo.GetRunByIdAsync(runId, runCts.Token);
            if (run == null) return;

            var instanceId = Environment.MachineName;
            await writer.MarkRunningAsync(runId, instanceId);
            await writer.AppendAsync(runId, ChatRunEventType.RunStarted, "");

            var token = tokenStore.Take(runId);
            var stream = streamClient.StreamAsync(runId, run.SessionId, run.UserMessage, token, runCts.Token);

            var lastFlush = DateTime.UtcNow;
            var lastHeartbeat = DateTime.UtcNow;
            segmentStartedAt = DateTime.UtcNow;

            await foreach (var evt in stream.WithCancellation(runCts.Token))
            {
                if (runCts.Token.IsCancellationRequested) break;

                if (evt.Type == ChatRunEventType.TextDelta)
                {
                    var content = evt.Payload;
                    fullOutput.Append(content);
                    chunkBuffer.Append(content);

                    if (chunkBuffer.Length >= FlushCharThreshold
                        || (DateTime.UtcNow - lastFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var chunkToFlush = chunkBuffer.ToString();
                        if (!string.IsNullOrEmpty(chunkToFlush))
                        {
                            await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                            await writer.AppendAsync(runId, evt.Type, chunkToFlush);
                            chunkBuffer.Clear();
                        }
                        lastFlush = DateTime.UtcNow;
                    }
                }
                else
                {
                    if (chunkBuffer.Length > 0)
                    {
                        await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                        await writer.AppendAsync(runId, ChatRunEventType.TextDelta, chunkBuffer.ToString());
                        chunkBuffer.Clear();
                    }

                    if (evt.Type == ChatRunEventType.TurnBoundary)
                    {
                        await writer.AppendSegmentAsync(runId, fullOutput.ToString(), segmentStartedAt);
                        fullOutput.Clear();
                        await writer.FlushPartialOutputAsync(runId, "");
                        segmentStartedAt = DateTime.UtcNow;
                    }
                    else if (evt.Type != "done")
                    {
                        await writer.AppendAsync(runId, evt.Type, evt.Payload);
                    }
                }

                if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds >= 15)
                {
                    await writer.UpdateHeartbeatAsync(runId);
                    lastHeartbeat = DateTime.UtcNow;
                }
            }

            if (chunkBuffer.Length > 0)
            {
                await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                await writer.AppendAsync(runId, ChatRunEventType.TextDelta, chunkBuffer.ToString());
            }

            if (runCts.Token.IsCancellationRequested)
            {
                await writer.CancelAsync(runId, fullOutput.ToString(), segmentStartedAt);
            }
            else
            {
                await writer.CompleteAsync(runId, fullOutput.ToString(), segmentStartedAt);
            }
        }
        catch (OperationCanceledException)
        {
            await writer.CancelAsync(runId, fullOutput.ToString(), segmentStartedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run {RunId}", runId);
            await writer.FailAsync(runId, ex);
        }
        finally
        {
            cancellationRegistry.Unregister(runId);
        }
    }
}
