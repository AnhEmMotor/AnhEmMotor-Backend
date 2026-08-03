using Application.Interfaces.Repositories.StoreChat;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatUpdateRepository(ApplicationDBContext context) : IStoreChatUpdateRepository
{
    public void UpdateSession(StoreChatSession session)
    {
        context.StoreChatSessions.Update(session);
    }

    // Race-safe: WHERE loại trừ "Human do người khác" đảm bảo chỉ request nào tới trước mới thắng —
    // giống idiom ExecuteUpdateAsync đã dùng ở ChatRunWriter, không cần thêm cột Version. Cho phép
    // nhận từ Ai/Waiting (gộp bước "Nhận" cũ vào gửi tin nhắn đầu tiên) hoặc gửi tiếp khi đã là mình.
    public async Task<bool> TryAssignStaffAsync(Guid sessionId, Guid staffId, CancellationToken cancellationToken = default)
    {
        // Gộp cả LastMessageAt vào cùng 1 ExecuteUpdateAsync — tránh ghi đè Mode/AssignedStaffId vừa
        // set ở đây bằng một UpdateSession(session) riêng với entity đã cũ (session fetch trước đó
        // không biết tới thay đổi race-safe này).
        var affected = await context.StoreChatSessions
            .Where(s => s.Id == sessionId && (s.Mode != StoreChatMode.Human || s.AssignedStaffId == staffId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Mode, StoreChatMode.Human)
                .SetProperty(x => x.AssignedStaffId, staffId)
                .SetProperty(x => x.LastMessageAt, DateTime.UtcNow), cancellationToken);
        return affected > 0;
    }

    public async Task<bool> TryReleaseAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var affected = await context.StoreChatSessions
            .Where(s => s.Id == sessionId && s.Mode == StoreChatMode.Human)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Mode, StoreChatMode.Ai)
                .SetProperty(x => x.AssignedStaffId, (Guid?)null), cancellationToken);
        return affected > 0;
    }

    public async Task TouchLastMessageAtAsync(Guid sessionId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        await context.StoreChatSessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastMessageAt, timestamp), cancellationToken);
    }
}
