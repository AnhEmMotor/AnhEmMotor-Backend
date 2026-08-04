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

    public async Task<bool> TryAssignStaffAsync(
        Guid sessionId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var affected = await context.StoreChatSessions
            .Where(s => s.Id == sessionId && (s.Mode != StoreChatMode.Human || s.AssignedStaffId == staffId))
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(x => x.Mode, StoreChatMode.Human)
                    .SetProperty(x => x.AssignedStaffId, staffId)
                    .SetProperty(x => x.LastMessageAt, DateTime.UtcNow),
                cancellationToken);
        return affected > 0;
    }

    public async Task<bool> TryReleaseAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var affected = await context.StoreChatSessions
            .Where(s => s.Id == sessionId && s.Mode == StoreChatMode.Human)
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(x => x.Mode, StoreChatMode.Ai)
                    .SetProperty(x => x.AssignedStaffId, (Guid?)null),
                cancellationToken);
        return affected > 0;
    }

    public async Task TouchLastMessageAtAsync(
        Guid sessionId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        await context.StoreChatSessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastMessageAt, timestamp), cancellationToken);
    }
}
