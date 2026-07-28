using Application.Interfaces.Repositories.Chat;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Chat;

public class ChatReadRepository(ApplicationDBContext context) : IChatReadRepository
{
    public async Task<List<ChatSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatRun?> GetActiveRunForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.ChatRuns
            .Include(r => r.Session)
            .Where(r => r.Session!.UserId == userId && (r.Status == Domain.Constants.ChatRunStatus.Pending || r.Status == Domain.Constants.ChatRunStatus.Running))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChatRun?> GetRunByIdAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        return await context.ChatRuns
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
    }

    public async Task<List<ChatRunEvent>> GetRunEventsAsync(Guid runId, long afterSeq, CancellationToken cancellationToken = default)
    {
        return await context.ChatRunEvents
            .Where(e => e.RunId == runId && e.Seq > afterSeq)
            .OrderBy(e => e.Seq)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountSteeringMessagesAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        return await context.ChatMessages
            .CountAsync(m => m.RunId == runId && m.IsSteering, cancellationToken);
    }
}
