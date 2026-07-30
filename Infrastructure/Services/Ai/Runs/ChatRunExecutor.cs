using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.Extensions.Configuration;
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
    // Không batching text_delta — forward ngay từng chunk model sinh ra, nhanh nhất có thể tới
    // FE, đúng tốc độ AI sinh token. Đánh đổi: nhiều lần ghi DB/SignalR hơn khi model trả chunk
    // nhỏ; nếu cần tối ưu lại, đó là việc của Stage 14 (Performance), không phải chặn tự nhiên.
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

    private sealed record RunMetaPayload(
        [property: JsonPropertyName("toolRegistryFingerprint")] string? ToolRegistryFingerprint,
        [property: JsonPropertyName("modelUsed")] string? ModelUsed);

    // Token còn đủ 5 phút (đúng bằng thời lượng chạy tối đa của 1 run) thì giữ nguyên; nếu không,
    // ký lại giữ nguyên claim với hạn mới — tránh 401 giữa run khi JWT gần hết hạn lúc pickup (17.9/E1).
    public static string EnsureFreshToken(string token, ITokenManagerService tokenManager, TimeSpan minRemaining)
    {
        DateTime validTo;
        try
        {
            validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        }
        catch (ArgumentException)
        {
            return token;
        }
        if (validTo - DateTime.UtcNow >= minRemaining)
        {
            return token;
        }
        return tokenManager.RefreshAccessToken(
            token, DateTimeOffset.UtcNow.AddMinutes(tokenManager.GetAccessTokenExpiryMinutes()));
    }

    // Host gọi StopAsync khi app shutdown (ApplicationStopping) — đợi các run đang chạy
    // huỷ xong (catch OperationCanceledException ở ProcessRunAsync) trước khi thoát. Không còn
    // buffer nội bộ để mất vì mỗi text_delta đã được ghi ngay khi tới.
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
        var tokenManager = scope.ServiceProvider.GetRequiredService<ITokenManagerService>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        runCts.CancelAfter(TimeSpan.FromMinutes(5));

        cancellationRegistry.Register(runId, runCts);

        // Khai báo ngoài try để catch (khi bị huỷ giữa chừng) vẫn lưu được phần đã sinh ra.
        var fullOutput = new StringBuilder();
        var segmentStartedAt = DateTime.UtcNow;

        try
        {
            var run = await readRepo.GetRunByIdAsync(runId, runCts.Token);
            if (run == null) return;

            var instanceId = Environment.MachineName;
            await writer.MarkRunningAsync(runId, instanceId);
            await writer.AppendAsync(runId, ChatRunEventType.RunStarted, "");

            var token = EnsureFreshToken(tokenStore.Take(runId), tokenManager, TimeSpan.FromMinutes(5));
            var stream = streamClient.StreamAsync(runId, run.SessionId, run.UserMessage, token, runCts.Token);

            var lastHeartbeat = DateTime.UtcNow;
            segmentStartedAt = DateTime.UtcNow;

            await foreach (var evt in stream.WithCancellation(runCts.Token))
            {
                if (runCts.Token.IsCancellationRequested) break;

                if (evt.Type == ChatRunEventType.TextDelta)
                {
                    fullOutput.Append(evt.Payload);
                    await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                }
                else if (evt.Type == ChatRunEventType.TurnBoundary)
                {
                    await writer.AppendSegmentAsync(runId, fullOutput.ToString(), segmentStartedAt);
                    fullOutput.Clear();
                    await writer.FlushPartialOutputAsync(runId, "");
                    segmentStartedAt = DateTime.UtcNow;
                }
                else if (evt.Type == ChatRunEventType.MessageCorrection)
                {
                    // Guardrail (13.7) phát hiện câu trả lời đã stream sai sau khi sinh xong —
                    // thay TOÀN BỘ nội dung đoạn hiện tại, không phải append, để cả lịch sử lưu
                    // và FE hiển thị đều dùng bản đã sửa, không chỉ FE.
                    fullOutput.Clear();
                    fullOutput.Append(evt.Payload);
                    await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                }
                else if (evt.Type == ChatRunEventType.RunMeta)
                {
                    var meta = JsonSerializer.Deserialize<RunMetaPayload>(evt.Payload);
                    await writer.SetRunMetaAsync(runId, meta?.ToolRegistryFingerprint, meta?.ModelUsed);
                    var configuredModel = configuration["AISetup:Model"];
                    if (!string.IsNullOrEmpty(meta?.ModelUsed) && !string.IsNullOrEmpty(configuredModel)
                        && meta.ModelUsed != configuredModel)
                    {
                        logger.LogError(
                            "ModelUsed lệch với AISetup:Model: run {RunId} dùng {Used}, cấu hình {Configured}",
                            runId, meta.ModelUsed, configuredModel);
                    }
                }
                else if (evt.Type != "done")
                {
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                }

                if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds >= 15)
                {
                    await writer.UpdateHeartbeatAsync(runId);
                    lastHeartbeat = DateTime.UtcNow;
                }
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
