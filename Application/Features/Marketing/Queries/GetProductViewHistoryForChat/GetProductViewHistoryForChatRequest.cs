namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public class GetProductViewHistoryForChatRequest
{
    public string? VisitorKey { get; set; }
    public Guid? CustomerId { get; set; }
    public int Limit { get; set; } = 10;
}
