using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatDeleteRepository(ApplicationDBContext context) : IStoreChatDeleteRepository
{
    // Xoá mềm (theo quy ước chung BaseEntity — ApplicationDBContext.SaveChangesAsync tự chuyển
    // Remove() thành set DeletedAt), ẩn khỏi mọi truy vấn qua query filter global. Xoá cả tin nhắn
    // cùng lúc để nhất quán — nếu sau này khôi phục phiên thì tin nhắn cũng còn nguyên.
    public async Task DeleteSessionAsync(StoreChatSession session, CancellationToken cancellationToken = default)
    {
        var messages = await context.StoreChatMessages
            .Where(m => m.SessionId == session.Id)
            .ToListAsync(cancellationToken);
        context.StoreChatMessages.RemoveRange(messages);
        context.StoreChatSessions.Remove(session);
    }
}
