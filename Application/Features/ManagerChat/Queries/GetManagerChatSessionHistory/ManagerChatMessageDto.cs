using Application.DTOs.Chat;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessionHistory;

public class ManagerChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public bool IsSteering { get; set; }
    public List<ChatMessageToolCallDto>? ToolCalls { get; set; }
}
