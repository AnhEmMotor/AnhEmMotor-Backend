namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public record ChatOrderStatusDto
{
    public int OrderId { get; init; }

    public string? StatusId { get; init; }

    public string? CustomerName { get; init; }

    public string? PaymentMethod { get; init; }

    public string? PaymentStatus { get; init; }

    public decimal Total { get; init; }

    public decimal? PaidAmount { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? LastStatusChangedAt { get; init; }
}
