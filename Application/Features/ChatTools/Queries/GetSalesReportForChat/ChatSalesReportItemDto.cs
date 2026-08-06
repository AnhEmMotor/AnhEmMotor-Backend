namespace Application.Features.ChatTools.Queries.GetSalesReportForChat;

public record ChatSalesReportItemDto
{
    public int OrderId { get; init; }

    public string? CustomerName { get; init; }

    public string? StatusId { get; init; }

    public string? PaymentStatus { get; init; }

    public decimal Total { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}
