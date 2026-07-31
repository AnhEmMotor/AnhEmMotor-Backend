namespace Application.Features.ChatTools.Queries.GetRecentTransactionsForChat;

public record ChatRecentTransactionDto
{
    public DateTime Timestamp { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public bool IsRevenue { get; init; }

    public string Status { get; init; } = string.Empty;

    public string StaffName { get; init; } = string.Empty;
}
