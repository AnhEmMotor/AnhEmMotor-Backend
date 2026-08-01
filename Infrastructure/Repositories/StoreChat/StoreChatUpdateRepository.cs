using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.StoreChat;

public class StoreChatUpdateRepository(ApplicationDBContext context) : IStoreChatUpdateRepository
{
    public void UpdateSession(StoreChatSession session)
    {
        context.StoreChatSessions.Update(session);
    }
}
