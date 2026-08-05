namespace Application.Features.ChatTools.Queries.GetRevenueAnalysisForChat;

public record ChatRevenueAnalysisDto
{
    public decimal TodayRevenue { get; init; }

    public decimal MonthlyRevenue { get; init; }

    public decimal TodayProfit { get; init; }

    public decimal MonthlyProfit { get; init; }

    public IReadOnlyList<ChatDailyRevenueItemDto> RevenueTrend { get; init; } = [];

    public IReadOnlyList<ChatTopProductRevenueItemDto> TopProducts { get; init; } = [];

    public IReadOnlyList<ChatBrandRevenueItemDto> BrandRevenueDistribution { get; init; } = [];

    public string Currency { get; init; } = "VND";
}

public record ChatDailyRevenueItemDto
{
    public DateOnly ReportDay { get; init; }

    public decimal TotalRevenue { get; init; }
}

public record ChatTopProductRevenueItemDto
{
    public string? ProductName { get; init; }

    public int UnitsSold { get; init; }

    public decimal Revenue { get; init; }
}

public record ChatBrandRevenueItemDto
{
    public string? BrandName { get; init; }

    public decimal Revenue { get; init; }

    public int QuantitySold { get; init; }
}
