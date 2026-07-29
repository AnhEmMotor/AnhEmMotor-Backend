using Application.DTOs.Chat;

namespace Application.Interfaces.Services;

public enum PendingSteeringAppendResult
{
    Appended,
    RunNotActive,
    TooMany,
    Conflict,
}

public interface IChatRunWriter
{
    public Task<long> AppendAsync(Guid runId, string type, object payload);
    public Task MarkRunningAsync(Guid runId, string instanceId);
    /// <summary>segmentStartedAt: mốc bắt đầu đoạn hiện tại — dùng để tra ChatRunEvent (tool_start) trong đúng khoảng thời gian của đoạn này, gắn vào ChatMessage.ToolCallsJson.</summary>
    public Task CompleteAsync(Guid runId, string finalOutput, DateTime segmentStartedAt);

    /// <summary>segmentStartedAt: xem CompleteAsync.</summary>
    public Task CancelAsync(Guid runId, string finalOutput, DateTime segmentStartedAt);
    public Task FailAsync(Guid runId, Exception ex);
    public Task UpdateHeartbeatAsync(Guid runId);
    public Task FlushPartialOutputAsync(Guid runId, string partialOutput);

    /// <summary>Chốt đoạn trả lời hiện tại thành 1 ChatMessage riêng và ghi mốc ranh giới lượt — dùng khi steering được hấp thụ giữa run, để mỗi lượt hiển thị thành 1 bong bóng AI riêng.</summary>
    public Task<long> AppendSegmentAsync(Guid runId, string segmentOutput, DateTime segmentStartedAt);

    /// <summary>Thêm 1 tin nhắn steering vào hàng chờ, atomic (compare-and-swap trên PendingSteering).</summary>
    public Task<PendingSteeringAppendResult> AppendPendingSteeringAsync(Guid runId, SteeringQueueItem item, int maxPending);

    /// <summary>Đọc và xoá toàn bộ hàng chờ steering, atomic. Rỗng nếu không có gì hoặc vừa bị đọc bởi lời gọi khác.</summary>
    public Task<List<SteeringQueueItem>> PullPendingSteeringAsync(Guid runId);
}
