using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatInsertRepository
{
    public void AddSession(ChatSession session);
    public void AddMessage(ChatMessage message);
}
