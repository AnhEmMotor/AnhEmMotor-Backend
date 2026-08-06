namespace Application.Features.ChatTools.Queries.ListWorkshopPaymentsForChat;

public class ChatWorkshopPaymentListItemDto
{
    public int PaymentId { get; init; }

    public string PaymentNumber { get; init; } = string.Empty;

    public string SourceType { get; init; } = string.Empty;

    public int SourceId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string CustomerPhone { get; init; } = string.Empty;

    public string? VehicleInfo { get; init; }

    public decimal TotalAmount { get; init; }

    public string PaymentMethod { get; init; } = string.Empty;

    public string PaymentStatus { get; init; } = string.Empty;

    public DateTimeOffset? PaidAt { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}
