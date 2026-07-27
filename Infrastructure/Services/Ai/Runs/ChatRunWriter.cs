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
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run == null) throw new InvalidOperationException("Run not found");

        run.LastSeq++;
        var seq = run.LastSeq;

        var payloadStr = payload is string s ? s : JsonSerializer.Serialize(payload);

        var evt = new ChatRunEvent
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            Seq = seq,
            Type = type,
            Payload = payloadStr,
            CreatedAt = DateTime.UtcNow
        };

        context.ChatRunEvents.Add(evt);
        await context.SaveChangesAsync();

        eventBus.Publish(runId, new ChatRunEventDto(seq, type, payloadStr));

        return seq;
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

    public async Task FlushPartialOutputAsync(Guid runId, string partialOutput)
    {
        var run = await context.ChatRuns.FirstOrDefaultAsync(r => r.Id == runId);
        if (run != null)
        {
            run.PartialOutput = partialOutput;
            await context.SaveChangesAsync();
        }
    }
}
