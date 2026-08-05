using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatInsertRepository(ApplicationDBContext context) : IStoreChatInsertRepository
{
    public void AddSession(StoreChatSession session)
    {
        context.StoreChatSessions.Add(session);
    }

    public void AddMessage(StoreChatMessage message)
    {
        context.StoreChatMessages.Add(message);
    }
}
