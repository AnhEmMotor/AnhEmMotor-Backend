namespace Application.Features.ManagerChat.Commands.CreateManagerChatSession;

public class CreateManagerChatSessionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public List<object> Messages { get; set; } = [];
}
