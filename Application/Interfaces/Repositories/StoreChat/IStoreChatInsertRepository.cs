using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatInsertRepository
{
    public void AddSession(StoreChatSession session);
    public void AddMessage(StoreChatMessage message);
}
