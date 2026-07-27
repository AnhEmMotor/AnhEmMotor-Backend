using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatReadRepository
{
    public Task<List<ChatSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<ChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    public Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    public Task<ChatRun?> GetActiveRunForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<ChatRun?> GetRunByIdAsync(Guid runId, CancellationToken cancellationToken = default);
    public Task<List<ChatRunEvent>> GetRunEventsAsync(Guid runId, long afterSeq, CancellationToken cancellationToken = default);
}
