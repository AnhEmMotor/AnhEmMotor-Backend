using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatUpdateRepository
{
    public void UpdateSession(StoreChatSession session);

    /// <summary>
    /// Tự nhận phiên khi gửi tin nhắn đầu tiên (Ai/Waiting -> Human, hoặc giữ nguyên nếu đã là mình) —
    /// an toàn khi 2 nhân viên cùng gửi gần như đồng thời, chỉ 1 người thắng.
    /// </summary>
    public Task<bool> TryAssignStaffAsync(Guid sessionId, Guid staffId, CancellationToken cancellationToken = default);

    /// <summary>Chuyển Human -> Ai, xoá AssignedStaffId.</summary>
    public Task<bool> TryReleaseAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật CHỈ LastMessageAt — dùng thay cho UpdateSession(session) ở nơi session được load TRƯỚC
    /// một thao tác có thể đổi Mode ở DbContext khác xen giữa (vd. AI gọi tool escalate_to_staff giữa
    /// lúc đang sinh câu trả lời), tránh ghi đè Mode cũ trong bộ nhớ lên giá trị mới vừa set.
    /// </summary>
    public Task TouchLastMessageAtAsync(Guid sessionId, DateTime timestamp, CancellationToken cancellationToken = default);
}
