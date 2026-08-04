using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunExecutor(
    IChatRunQueue queue,
    IServiceProvider serviceProvider,
    IChatRunCancellationRegistry cancellationRegistry,
    ILogger<ChatRunExecutor> logger) : BackgroundService
{
    private readonly SemaphoreSlim _semaphore = new(10);
    private readonly ConcurrentDictionary<Guid, Task> _inFlightRuns = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var runId in queue.ReadAllAsync(stoppingToken))
        {
            await _semaphore.WaitAsync(stoppingToken);
            var runTask = Task.Run(
                async () =>
                {
                    try
                    {
                        await ProcessRunAsync(runId, stoppingToken);
                    } catch (Exception ex)
                    {
                        logger.LogError(ex, "Lỗi khi xử lý run {RunId}", runId);
                    } finally
                    {
                        _semaphore.Release();
                    }
                },
                stoppingToken);
            _inFlightRuns[runId] = runTask;
            _ = runTask.ContinueWith(_ => ((IDictionary<Guid, Task>)_inFlightRuns).Remove(runId), TaskScheduler.Default);
        }
    }

    private sealed record RunMetaPayload(
        [property: JsonPropertyName("toolRegistryFingerprint")] string? ToolRegistryFingerprint,
        [property: JsonPropertyName("modelUsed")] string? ModelUsed);

    public static string EnsureFreshToken(string token, ITokenManagerService tokenManager, TimeSpan minRemaining)
    {
        DateTime validTo;
        try
        {
            validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        } catch (ArgumentException)
        {
            return token;
        }
        if (validTo - DateTime.UtcNow >= minRemaining)
        {
            return token;
        }
        return tokenManager.RefreshAccessToken(
            token,
            DateTimeOffset.UtcNow.AddMinutes(tokenManager.GetAccessTokenExpiryMinutes()));
    }

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
        var fullOutput = new StringBuilder();
        var segmentStartedAt = DateTime.UtcNow;
        try
        {
            var run = await readRepo.GetRunByIdAsync(runId, runCts.Token);
            if (run == null)
                return;
            var instanceId = Environment.MachineName;
            await writer.MarkRunningAsync(runId, instanceId);
            await writer.AppendAsync(runId, ChatRunEventType.RunStarted, string.Empty);
            var token = EnsureFreshToken(tokenStore.Take(runId), tokenManager, TimeSpan.FromMinutes(5));
            var stream = streamClient.StreamAsync(runId, run.SessionId, run.UserMessage, token, runCts.Token);
            var lastHeartbeat = DateTime.UtcNow;
            segmentStartedAt = DateTime.UtcNow;
            var isAwaitingApproval = false;
            await foreach (var evt in stream.WithCancellation(runCts.Token))
            {
                if (runCts.Token.IsCancellationRequested)
                    break;
                if (evt.Type == ChatRunEventType.PlanReady)
                {
                    isAwaitingApproval = true;
                }
                if (evt.Type == ChatRunEventType.TextDelta)
                {
                    fullOutput.Append(evt.Payload);
                    await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                } else if (evt.Type == ChatRunEventType.TurnBoundary)
                {
                    await writer.AppendSegmentAsync(runId, fullOutput.ToString(), segmentStartedAt);
                    fullOutput.Clear();
                    await writer.FlushPartialOutputAsync(runId, string.Empty);
                    segmentStartedAt = DateTime.UtcNow;
                } else if (evt.Type == ChatRunEventType.MessageCorrection)
                {
                    fullOutput.Clear();
                    fullOutput.Append(evt.Payload);
                    await writer.FlushPartialOutputAsync(runId, fullOutput.ToString());
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                } else if (evt.Type == ChatRunEventType.RunMeta)
                {
                    var meta = JsonSerializer.Deserialize<RunMetaPayload>(evt.Payload);
                    await writer.SetRunMetaAsync(runId, meta?.ToolRegistryFingerprint, meta?.ModelUsed);
                    var configuredModel = configuration["AISetup:Model"];
                    if (!string.IsNullOrEmpty(meta?.ModelUsed) &&
                        !string.IsNullOrEmpty(configuredModel) &&
                        meta.ModelUsed != configuredModel)
                    {
                        logger.LogError(
                            "ModelUsed lệch với AISetup:Model: run {RunId} dùng {Used}, cấu hình {Configured}",
                            runId,
                            meta.ModelUsed,
                            configuredModel);
                    }
                } else if (evt.Type == ChatRunEventType.Thinking)
                {
                    await writer.AppendAsync(runId, evt.Type, evt.Payload);
                } else if (evt.Type != "done")
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
            } else if (isAwaitingApproval)
            {
                await writer.AwaitingApprovalAsync(runId, fullOutput.ToString(), segmentStartedAt);
            } else
            {
                await writer.CompleteAsync(runId, fullOutput.ToString(), segmentStartedAt);
            }
        } catch (OperationCanceledException)
        {
            await writer.CancelAsync(runId, fullOutput.ToString(), segmentStartedAt);
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run {RunId}", runId);
            await writer.FailAsync(runId, ex);
        } finally
        {
            cancellationRegistry.Unregister(runId);
        }
    }
}
