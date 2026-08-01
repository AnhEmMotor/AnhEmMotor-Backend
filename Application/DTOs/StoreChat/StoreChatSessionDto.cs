namespace Application.DTOs.StoreChat;

public class StoreChatSessionDto
{
    public Guid Id { get; set; }
    public string VisitorKey { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
}
