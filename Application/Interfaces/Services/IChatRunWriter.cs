namespace Application.Interfaces.Services;

public interface IChatRunWriter
{
    Task<long> AppendAsync(Guid runId, string type, object payload);
    Task MarkRunningAsync(Guid runId, string instanceId);
    Task CompleteAsync(Guid runId, string finalOutput);
    Task CancelAsync(Guid runId, string finalOutput);
    Task FailAsync(Guid runId, Exception ex);
    Task UpdateHeartbeatAsync(Guid runId);
    Task FlushPartialOutputAsync(Guid runId, string partialOutput);
}
