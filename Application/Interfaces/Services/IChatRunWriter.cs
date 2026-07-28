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
    Task<long> AppendAsync(Guid runId, string type, object payload);
    Task MarkRunningAsync(Guid runId, string instanceId);
    Task CompleteAsync(Guid runId, string finalOutput);
    Task CancelAsync(Guid runId, string finalOutput);
    Task FailAsync(Guid runId, Exception ex);
    Task UpdateHeartbeatAsync(Guid runId);
    Task FlushPartialOutputAsync(Guid runId, string partialOutput);

    /// <summary>Chốt đoạn trả lời hiện tại thành 1 ChatMessage riêng và ghi mốc ranh giới lượt — dùng khi steering được hấp thụ giữa run, để mỗi lượt hiển thị thành 1 bong bóng AI riêng.</summary>
    Task<long> AppendSegmentAsync(Guid runId, string segmentOutput, DateTime segmentStartedAt);

    /// <summary>Thêm 1 tin nhắn steering vào hàng chờ, atomic (compare-and-swap trên PendingSteering).</summary>
    Task<PendingSteeringAppendResult> AppendPendingSteeringAsync(Guid runId, SteeringQueueItem item, int maxPending);

    /// <summary>Đọc và xoá toàn bộ hàng chờ steering, atomic. Rỗng nếu không có gì hoặc vừa bị đọc bởi lời gọi khác.</summary>
    Task<List<SteeringQueueItem>> PullPendingSteeringAsync(Guid runId);
}
