using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatUpdateRepository
{
    public void UpdateSession(ChatSession session);
}
