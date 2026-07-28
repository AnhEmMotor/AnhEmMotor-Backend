using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatReadRepository
{
    public Task<List<ChatSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<ChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    public Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
