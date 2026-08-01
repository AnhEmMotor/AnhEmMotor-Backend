using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatUpdateRepository
{
    public void UpdateSession(StoreChatSession session);
}
