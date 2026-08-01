namespace Application.DTOs.StoreChat;

public class StoreChatMessageDto
{
    public Guid Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CardsJson { get; set; }
}
