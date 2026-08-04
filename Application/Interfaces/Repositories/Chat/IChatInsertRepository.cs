using Domain.Entities;

namespace Application.Interfaces.Repositories.Chat;

public interface IChatInsertRepository
{
    public void AddSession(ChatSession session);
    public void AddMessage(ChatMessage message);
    public void AddRun(ChatRun run);
    public void AddFeedback(ChatFeedback feedback);
    public void AddPlan(ChatPlan plan);
    public void AddTemplate(ChatPlanTemplate template);
}
