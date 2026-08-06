using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatDeleteRepository(ApplicationDBContext context) : IStoreChatDeleteRepository
{
    public async Task DeleteSessionAsync(StoreChatSession session, CancellationToken cancellationToken = default)
    {
        var messages = await context.StoreChatMessages
            .Where(m => m.SessionId == session.Id)
            .ToListAsync(cancellationToken);
        context.StoreChatMessages.RemoveRange(messages);
        context.StoreChatSessions.Remove(session);
    }
}
