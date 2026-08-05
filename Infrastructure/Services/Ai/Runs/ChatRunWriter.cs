using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunWriter(
    ApplicationDBContext context,
    IChatRunEventBus eventBus,
    IChatToolCatalogProvider catalogProvider) : IChatRunWriter
{
    private async Task<string?> BuildReasoningStepsJsonAsync(Guid runId, DateTime segmentStartedAt)
    {
        var segmentEndedAt = DateTime.UtcNow;
        var events = await context.ChatRunEvents
            .Where(
                e => e.RunId == runId &&
                    (e.Type == ChatRunEventType.Thinking ||
                        e.Type == ChatRunEventType.ToolStart ||
                        e.Type == ChatRunEventType.ToolEnd) &&
                    e.CreatedAt >= segmentStartedAt &&
                    e.CreatedAt <= segmentEndedAt)
            .OrderBy(e => e.Seq)
            .Select(e => new { e.Type, e.Payload })
            .ToListAsync();
        if (events.Count == 0)
            return null;
        var labelByName = catalogProvider.GetCatalog().ToDictionary(e => e.Name, e => e.Label);
        var steps = BuildReasoningSteps(events.Select(e => (e.Type, e.Payload)), labelByName);
        return steps.Count == 0 ? null : JsonSerializer.Serialize(steps);
    }

    /// <summary>
    /// Gộp các event thinking/tool_start/tool_end (theo đúng thứ tự Seq) thành danh sách ChatReasoningStepDto — cùng
    /// hình dạng "reasoningSteps" mà FE dựng live lúc đang stream. Tách riêng khỏi truy vấn DB để test trực tiếp không
    /// cần EF InMemory.
    /// </summary>
    public static List<ChatReasoningStepDto> BuildReasoningSteps(
        IEnumerable<(string Type, string Payload)> events,
        Dictionary<string, string> labelByName)
    {
        var steps = new List<ChatReasoningStepDto>();
        foreach (var (type, payload) in events)
        {
            if (type == ChatRunEventType.Thinking)
            {
                var text = ParseThinkingPayload(payload);
                if (!string.IsNullOrEmpty(text))
                {
                    steps.Add(new ChatReasoningStepDto("thinking", Text: text));
                }
            } else if (type == ChatRunEventType.ToolStart)
            {
                var (name, summary, argsPreview) = ParseToolStartPayload(payload);
                steps.Add(
                    new ChatReasoningStepDto(
                        "tool",
                        Name: name,
                        Label: labelByName.GetValueOrDefault(name, name),
                        Summary: summary,
                        Status: "running",
                        ArgsPreview: argsPreview));
            } else
            {
                var end = ParseToolEndPayload(payload);
                var idx = steps.FindLastIndex(s => s.Kind == "tool" && s.Name == end.Name && s.Status == "running");
                if (idx >= 0)
                {
                    steps[idx] = steps[idx] with
                    {
                        Summary = end.Summary ?? steps[idx].Summary,
                        Status = "done",
                        DurationMs = end.DurationMs,
                        ResultPreview = end.ResultPreview,
                        Truncated = end.Truncated,
                        TotalCount = end.TotalCount,
                        AsOf = end.AsOf,
                        Warnings = end.Warnings,
                        FiltersApplied = end.FiltersApplied,
                    };
                }
            }
        }
        return steps;
    }

    private static string? ParseThinkingPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() : payload;
        } catch (JsonException)
        {
            return payload;
        }
    }

    private static (string Name, string? Summary, JsonElement? ArgsPreview) ParseToolStartPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var name = root.TryGetProperty("name", out var n) ? n.GetString() ?? payload : payload;
            var summary = root.TryGetProperty("summary", out var s) ? s.GetString() : null;
            JsonElement? argsPreview = root.TryGetProperty("argsPreview", out var ap) ? ap.Clone() : null;
            return (name, string.IsNullOrEmpty(summary) ? null : summary, argsPreview);
        } catch (JsonException)
        {
            return (payload, null, null);
        }
    }

    private sealed record ToolEndPayloadData(
        string Name,
        string? Summary,
        int? DurationMs,
        JsonElement? ResultPreview,
        bool? Truncated,
        int? TotalCount,
        string? AsOf,
        List<string>? Warnings,
        Dictionary<string, string>? FiltersApplied);

    private static ToolEndPayloadData ParseToolEndPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var name = root.TryGetProperty("name", out var n) ? n.GetString() ?? payload : payload;
            var summary = root.TryGetProperty("summary", out var s) ? s.GetString() : null;
            int? durationMs = root.TryGetProperty("durationMs", out var d) && d.TryGetInt32(out var dv) ? dv : null;
            JsonElement? resultPreview = root.TryGetProperty("resultPreview", out var rp) ? rp.Clone() : null;
            bool? truncated = root.TryGetProperty("truncated", out var tr) ? tr.GetBoolean() : null;
            int? totalCount = root.TryGetProperty("totalCount", out var tc) && tc.TryGetInt32(out var tcv) ? tcv : null;
            string? asOf = root.TryGetProperty("asOf", out var ao) ? ao.GetString() : null;
            List<string>? warnings = root.TryGetProperty("warnings", out var w)
                ? JsonSerializer.Deserialize<List<string>>(w.GetRawText())
                : null;
            Dictionary<string, string>? filtersApplied = root.TryGetProperty("filtersApplied", out var fa)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(fa.GetRawText())
                : null;
            return new ToolEndPayloadData(
                name,
                summary,
                durationMs,
                resultPreview,
                truncated,
                totalCount,
                asOf,
                warnings,
                filtersApplied);
        } catch (JsonException)
        {
            return new ToolEndPayloadData(payload, null, null, null, null, null, null, null, null);
        }
    }

    public async Task<long> AppendAsync(Guid runId, string type, object payload)
    {
        var payloadStr = payload is string s ? s : JsonSerializer.Serialize(payload);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null)
                throw new InvalidOperationException("Run not found");
            var nextSeq = run.LastSeq + 1;
            var claimed = await context.ChatRuns
                .Where(r => r.Id == runId && r.LastSeq == run.LastSeq)
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.LastSeq, nextSeq));
            if (claimed == 0)
                continue;
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
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.Status, ChatRunStatus.Running)
                    .SetProperty(r => r.OwnerInstanceId, instanceId)
                    .SetProperty(r => r.HeartbeatAt, DateTime.UtcNow));
    }

    public async Task CompleteAsync(Guid runId, string finalOutput, DateTime segmentStartedAt)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null)
            return;
        run.Status = ChatRunStatus.Completed;
        run.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(finalOutput))
        {
            var now = DateTime.UtcNow;
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = finalOutput,
                RunId = runId,
                CreatedAt = now,
                ReasoningStepsJson = await BuildReasoningStepsJsonAsync(runId, segmentStartedAt),
                ReasoningElapsedSeconds = (now - segmentStartedAt).TotalSeconds
            };
            context.ChatMessages.Add(aiMessage);
        }
        await context.SaveChangesAsync();
        await AppendAsync(runId, ChatRunEventType.RunCompleted, string.Empty);
    }

    public async Task AwaitingApprovalAsync(Guid runId, string finalOutput, DateTime segmentStartedAt)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null)
            return;
        run.Status = ChatRunStatus.AwaitingApproval;
        if (!string.IsNullOrEmpty(finalOutput))
        {
            var now = DateTime.UtcNow;
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = finalOutput,
                RunId = runId,
                CreatedAt = now,
                ReasoningStepsJson = await BuildReasoningStepsJsonAsync(runId, segmentStartedAt),
                ReasoningElapsedSeconds = (now - segmentStartedAt).TotalSeconds
            };
            context.ChatMessages.Add(aiMessage);
        }
        await context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid runId, string finalOutput, DateTime segmentStartedAt)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null)
            return;
        run.Status = ChatRunStatus.Cancelled;
        run.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(finalOutput))
        {
            var now = DateTime.UtcNow;
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = run.SessionId,
                Role = ChatRole.Ai,
                Message = finalOutput,
                RunId = runId,
                CreatedAt = now,
                ReasoningStepsJson = await BuildReasoningStepsJsonAsync(runId, segmentStartedAt),
                ReasoningElapsedSeconds = (now - segmentStartedAt).TotalSeconds
            };
            context.ChatMessages.Add(aiMessage);
        }
        await context.SaveChangesAsync();
        await AppendAsync(runId, ChatRunEventType.RunCancelled, string.Empty);
    }

    public async Task FailAsync(Guid runId, Exception ex)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.Status, ChatRunStatus.Failed)
                    .SetProperty(r => r.ErrorCode, ex.Message)
                    .SetProperty(r => r.CompletedAt, DateTime.UtcNow));
        await AppendAsync(runId, ChatRunEventType.Error, ex.Message);
    }

    public async Task UpdateHeartbeatAsync(Guid runId)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.HeartbeatAt, DateTime.UtcNow));
        await AppendAsync(runId, ChatRunEventType.RunHeartbeat, string.Empty);
    }

    public async Task<long> AppendSegmentAsync(Guid runId, string segmentOutput, DateTime segmentStartedAt)
    {
        var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null)
            throw new InvalidOperationException("Run not found");
        if (!string.IsNullOrEmpty(segmentOutput))
        {
            context.ChatMessages
                .Add(
                    new ChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = run.SessionId,
                        Role = ChatRole.Ai,
                        Message = segmentOutput,
                        RunId = runId,
                        CreatedAt = segmentStartedAt,
                        ReasoningStepsJson = await BuildReasoningStepsJsonAsync(runId, segmentStartedAt),
                        ReasoningElapsedSeconds = (DateTime.UtcNow - segmentStartedAt).TotalSeconds
                    });
            await context.SaveChangesAsync();
        }
        return await AppendAsync(runId, ChatRunEventType.TurnBoundary, string.Empty);
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

    public async Task SetRunMetaAsync(Guid runId, string? toolRegistryFingerprint, string? modelUsed)
    {
        await context.ChatRuns
            .Where(r => r.Id == runId)
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.ToolRegistryFingerprint, r => toolRegistryFingerprint ?? r.ToolRegistryFingerprint)
                    .SetProperty(r => r.ModelUsed, r => modelUsed ?? r.ModelUsed));
    }

    private static bool IsActive(string status) => status == ChatRunStatus.Running || status == ChatRunStatus.Pending;

    private static List<SteeringQueueItem> DeserializeSteering(string json) => string.IsNullOrEmpty(json)
        ? []
        : JsonSerializer.Deserialize<List<SteeringQueueItem>>(json) ?? [];

    public async Task<PendingSteeringAppendResult> AppendPendingSteeringAsync(
        Guid runId,
        SteeringQueueItem item,
        int maxPending)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null || !IsActive(run.Status))
                return PendingSteeringAppendResult.RunNotActive;
            var items = DeserializeSteering(run.PendingSteering);
            if (items.Count >= maxPending)
                return PendingSteeringAppendResult.TooMany;
            items.Add(item);
            var newJson = JsonSerializer.Serialize(items);
            var affected = await context.ChatRuns
                .Where(
                    r => r.Id == runId &&
                        r.PendingSteering == run.PendingSteering &&
                        (r.Status == ChatRunStatus.Running || r.Status == ChatRunStatus.Pending))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.PendingSteering, newJson));
            if (affected > 0)
                return PendingSteeringAppendResult.Appended;
        }
        return PendingSteeringAppendResult.Conflict;
    }

    public async Task<List<SteeringQueueItem>> PullPendingSteeringAsync(Guid runId)
    {
        var run = await context.ChatRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null || run.PendingSteering == "[]")
            return [];
        var items = DeserializeSteering(run.PendingSteering);
        if (items.Count == 0)
            return [];
        var affected = await context.ChatRuns
            .Where(r => r.Id == runId && r.PendingSteering == run.PendingSteering)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.PendingSteering, "[]"));
        if (affected == 0)
            return [];
        foreach (var item in items)
        {
            await AppendAsync(runId, ChatRunEventType.SteeringApplied, JsonSerializer.Serialize(item));
        }
        return items;
    }
}
