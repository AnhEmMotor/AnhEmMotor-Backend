namespace Application.Features.ChatTools.Queries.GetSalesSummaryForChat;

public record ChatDailyRevenueDto
{
    public DateOnly ReportDay { get; init; }

    public decimal TotalRevenue { get; init; }

    public string Currency { get; init; } = "VND";
}
