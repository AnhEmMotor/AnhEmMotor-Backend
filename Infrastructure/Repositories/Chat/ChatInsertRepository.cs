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

    public void AddRun(ChatRun run)
    {
        context.ChatRuns.Add(run);
    }

    public void AddFeedback(ChatFeedback feedback)
    {
        context.ChatFeedbacks.Add(feedback);
    }

    public void AddPlan(ChatPlan plan)
    {
        context.ChatPlans.Add(plan);
    }

    public void AddTemplate(ChatPlanTemplate template)
    {
        context.ChatPlanTemplates.Add(template);
    }
}
