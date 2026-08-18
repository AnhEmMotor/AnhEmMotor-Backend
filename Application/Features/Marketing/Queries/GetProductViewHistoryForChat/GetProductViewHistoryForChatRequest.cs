namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public class GetProductViewHistoryForChatRequest
{
    public string? VisitorKey { get; set; }
    public string? CustomerKeyword { get; set; }
    public int Limit { get; set; } = 10;
}
