using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatDeleteRepository
{
    public void DeleteSession(ChatSession session);
}
