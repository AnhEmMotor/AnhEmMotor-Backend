using Application.Interfaces.Repositories.Chat;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Chat;

public class ChatDeleteRepository(ApplicationDBContext context) : IChatDeleteRepository
{
    public void DeleteSession(ChatSession session)
    {
        context.ChatSessions.Remove(session);
    }
}
