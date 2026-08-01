using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatReadRepository
{
    public Task<StoreChatSession?> GetSessionByVisitorKeyAsync(string visitorKey, CancellationToken cancellationToken = default);
    public Task<StoreChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    public Task<List<StoreChatMessage>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
