using System.Text.Json;
using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunWriter(
    ApplicationDBContext context,
    IChatRunEventBus eventBus) : IChatRunWriter
{
    public async Task<long> AppendAsync(Guid runId, string type, object payload)
    {
        var payloadStr = payload is string s ? s : JsonSerializer.Serialize(payload);

        // ponytail: từ Stage 9, ChatRunExecutor và endpoint pull-steering có thể ghi event cho
        // CÙNG một run trên hai DbContext khác nhau cùng lúc — LastSeq++ trong bộ nhớ không còn
        // an toàn. Dùng compare-and-swap giống PendingSteering thay vì rowversion riêng.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null) throw new InvalidOperationException("Run not found");

            var nextSeq = run.LastSeq + 1;

            var claimed = await context.ChatRuns
                .Where(r => r.Id == runId && r.LastSeq == run.LastSeq)
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.LastSeq, nextSeq));

            if (claimed == 0) continue;

            var evt = new ChatRunEvent
            {
                Id = Guid.NewGuid(),
                RunId = runId,
                Seq = nextSeq,
                Type = type,
                Payload = payloadStr,
                CreatedAt = DateTime.UtcNow
            };

            context.ChatRunEvents.Add(evt);
            await context.SaveChangesAsync();

            eventBus.Publish(runId, new ChatRunEventDto(nextSeq, type, payloadStr));

            return nextSeq;
        }
        throw new InvalidOperationException($"Không thể ghi event cho run {runId} sau nhiều lần thử.");
    }

    public async Task MarkRunningAsync(Guid runId, string instanceId)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ChatRunStatus.Running)
                .SetProperty(r => r.OwnerInstanceId, instanceId));
    }

    public async Task CompleteAsync(Guid runId, string finalOutput)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null) return;

        run.Status = ChatRunStatus.Completed;
        run.CompletedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(finalOutput))
        {
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = finalOutput,
                RunId = runId,
                CreatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(aiMessage);
        }

        await context.SaveChangesAsync();
        await AppendAsync(runId, ChatRunEventType.RunCompleted, "");
    }

    public async Task CancelAsync(Guid runId, string finalOutput)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null) return;

        run.Status = ChatRunStatus.Cancelled;
        run.CompletedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(finalOutput))
        {
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = finalOutput,
                RunId = runId,
                CreatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(aiMessage);
        }

        await context.SaveChangesAsync();
        await AppendAsync(runId, ChatRunEventType.RunCancelled, "");
    }

    public async Task FailAsync(Guid runId, Exception ex)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ChatRunStatus.Failed)
                .SetProperty(r => r.ErrorCode, ex.Message)
                .SetProperty(r => r.CompletedAt, DateTime.UtcNow));

        await AppendAsync(runId, ChatRunEventType.Error, ex.Message);
    }

    public async Task UpdateHeartbeatAsync(Guid runId)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.HeartbeatAt, DateTime.UtcNow));
        
        await AppendAsync(runId, ChatRunEventType.RunHeartbeat, "");
    }

    public async Task<long> AppendSegmentAsync(Guid runId, string segmentOutput, DateTime segmentStartedAt)
    {
        var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null) throw new InvalidOperationException("Run not found");

        if (!string.IsNullOrEmpty(segmentOutput))
        {
            context.ChatMessages.Add(new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = segmentOutput,
                RunId = runId,
                CreatedAt = segmentStartedAt
            });
            await context.SaveChangesAsync();
        }

        return await AppendAsync(runId, ChatRunEventType.TurnBoundary, "");
    }

    public async Task FlushPartialOutputAsync(Guid runId, string partialOutput)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run != null)
        {
            run.PartialOutput = partialOutput;
            await context.SaveChangesAsync();
        }
    }

    private static bool IsActive(string status) =>
        status == ChatRunStatus.Running || status == ChatRunStatus.Pending;

    private static List<SteeringQueueItem> DeserializeSteering(string json) =>
        string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<SteeringQueueItem>>(json) ?? [];

    public async Task<PendingSteeringAppendResult> AppendPendingSteeringAsync(Guid runId, SteeringQueueItem item, int maxPending)
    {
        // ponytail: compare-and-swap trên chuỗi PendingSteering thay vì rowversion riêng —
        // đủ dùng vì cột này chỉ 1-3 phần tử, tranh chấp cực hiếm. Nâng cấp nếu throughput tăng.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null || !IsActive(run.Status)) return PendingSteeringAppendResult.RunNotActive;

            var items = DeserializeSteering(run.PendingSteering);
            if (items.Count >= maxPending) return PendingSteeringAppendResult.TooMany;

            items.Add(item);
            var newJson = JsonSerializer.Serialize(items);

            var affected = await context.ChatRuns
                .Where(r => r.Id == runId && r.PendingSteering == run.PendingSteering
                            && (r.Status == ChatRunStatus.Running || r.Status == ChatRunStatus.Pending))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.PendingSteering, newJson));

            if (affected > 0) return PendingSteeringAppendResult.Appended;
        }
        return PendingSteeringAppendResult.Conflict;
    }

    public async Task<List<SteeringQueueItem>> PullPendingSteeringAsync(Guid runId)
    {
        var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null || run.PendingSteering == "[]") return [];

        var items = DeserializeSteering(run.PendingSteering);
        if (items.Count == 0) return [];

        var affected = await context.ChatRuns
            .Where(r => r.Id == runId && r.PendingSteering == run.PendingSteering)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.PendingSteering, "[]"));

        // Bị đè bởi 1 lần append khác giữa lúc đọc và xoá — không trả gì, lần poll sau sẽ lấy trọn.
        if (affected == 0) return [];

        foreach (var item in items)
        {
            await AppendAsync(runId, ChatRunEventType.SteeringApplied,
                JsonSerializer.Serialize(item));
        }

        return items;
    }
}
