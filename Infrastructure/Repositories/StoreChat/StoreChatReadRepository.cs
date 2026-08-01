using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatReadRepository(ApplicationDBContext context) : IStoreChatReadRepository
{
    public async Task<StoreChatSession?> GetSessionByVisitorKeyAsync(string visitorKey, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatSessions
            .FirstOrDefaultAsync(s => s.VisitorKey == visitorKey, cancellationToken);
    }

    public async Task<StoreChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<List<StoreChatMessage>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
