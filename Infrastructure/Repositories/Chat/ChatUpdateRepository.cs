using Application.Interfaces.Repositories.Chat;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Chat;

public class ChatUpdateRepository(ApplicationDBContext context) : IChatUpdateRepository
{
    public void UpdateSession(ChatSession session)
    {
        context.ChatSessions.Update(session);
    }

}
