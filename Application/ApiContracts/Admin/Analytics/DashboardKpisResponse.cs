namespace Application.ApiContracts.Admin.Analytics
{
public record DashboardKpisResponse(
    string Period,
    string StartDate,
    string EndDate,
    List<CardItem> Cards,
    AlertsSummary Alerts);

public record CardItem(
    string Label,
    decimal Value,
    double Change,
    string Icon,
    string Unit);

public record AlertsSummary(
    FinancialAlerts Financial,
    InventoryAlerts Inventory,
    CustomerAlerts Customer,
    OperationsAlerts Operations);

public record FinancialAlerts(int DelayedLoans, bool LowRevenue);
public record InventoryAlerts(int LowStockVehicles, int LowStockParts);
public record CustomerAlerts(int NewComplaints, int MissedAppointments);
public record OperationsAlerts(int PendingOrders);

public record CustomerFunnelDto(string Stage, int Count);

public record ProductStructureDto(string Category, decimal RevenueShare);

public record SaleLeaderboardDto(string SaleName, decimal Revenue);

public record AnalyticsChartsResponse(
    List<CustomerFunnelDto> Funnel,
    List<ProductStructureDto> Structure,
    List<SaleLeaderboardDto> Leaderboard);
}