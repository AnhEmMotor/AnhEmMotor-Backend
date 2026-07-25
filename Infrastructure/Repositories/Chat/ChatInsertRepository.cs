using Application.Interfaces.Repositories.Chat;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Chat;

public class ChatInsertRepository(ApplicationDBContext context) : IChatInsertRepository
{
    public void AddSession(ChatSession session)
    {
        context.ChatSessions.Add(session);
    }

    public void AddMessage(ChatMessage message)
    {
        context.ChatMessages.Add(message);
    }
}
