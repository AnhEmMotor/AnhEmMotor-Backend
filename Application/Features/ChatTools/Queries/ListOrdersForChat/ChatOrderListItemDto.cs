namespace Application.Features.ChatTools.Queries.ListOrdersForChat;

public record ChatOrderListItemDto
{
    public int OrderId { get; init; }

    public string? CustomerName { get; init; }

    public string? StatusId { get; init; }

    public decimal Total { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTimeOffset? CreatedAt { get; init; }
}
