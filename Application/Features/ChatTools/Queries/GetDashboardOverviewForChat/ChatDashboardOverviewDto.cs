namespace Application.Features.ChatTools.Queries.GetDashboardOverviewForChat;

public record ChatDashboardOverviewDto
{
    public decimal TodayRevenue { get; init; }

    public decimal MonthlyRevenue { get; init; }

    public decimal TodayProfit { get; init; }

    public decimal MonthlyProfit { get; init; }

    public int PendingOrdersCount { get; init; }

    public int OverdueOrdersCount { get; init; }

    public int NewCustomersCount { get; init; }

    public int TodayVehiclesSold { get; init; }

    public int MonthlyVehiclesSold { get; init; }

    public int CurrentInventoryCount { get; init; }

    public decimal OverdueDebtAmount { get; init; }

    public string Currency { get; init; } = "VND";
}
