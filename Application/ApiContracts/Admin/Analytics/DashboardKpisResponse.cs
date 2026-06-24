using System.Collections.Generic;

namespace Application.ApiContracts.Admin.Analytics
{
    public record DashboardKpisResponse(
        decimal TotalRevenue, 
        int VehiclesSold, 
        int NewLeadsToday, 
        double TestDriveFillRate, 
        decimal PendingPipelineValue, 
        double MonthlyTargetProgress);

    public record CustomerFunnelDto(string Stage, int Count);
    public record ProductStructureDto(string Category, decimal RevenueShare);
    public record SaleLeaderboardDto(string SaleName, decimal Revenue);

    public record AnalyticsChartsResponse(
        List<CustomerFunnelDto> Funnel, 
        List<ProductStructureDto> Structure, 
        List<SaleLeaderboardDto> Leaderboard);
}
