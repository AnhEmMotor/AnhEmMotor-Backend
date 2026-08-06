using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatDeleteRepository
{
    public Task DeleteSessionAsync(StoreChatSession session, CancellationToken cancellationToken = default);
}
