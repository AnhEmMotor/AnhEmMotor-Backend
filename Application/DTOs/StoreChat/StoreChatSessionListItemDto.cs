namespace Application.DTOs.StoreChat;

public class StoreChatSessionListItemDto
{
    public Guid Id { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? CustomerName { get; set; }
    public Guid? PreviousSessionId { get; set; }
    public Guid? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
    public DateTime LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
}
