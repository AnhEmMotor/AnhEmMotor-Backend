using Application.ApiContracts.Statistical.Responses;

namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalReadRepository
{
    public Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken);

    public Task<IEnumerable<DailyRevenueDetailResponse>> GetDailyRevenueDetailAsync(
        DateOnly reportDay,
        int days,
        CancellationToken cancellationToken);

    public Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken);

    public Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken);

    public Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken);

    public Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken);

    public Task<IEnumerable<TopProductRevenueResponse>> GetTopProductsByRevenueAsync(
        int limit,
        CancellationToken cancellationToken);

    public Task<IEnumerable<BrandRevenueResponse>> GetBrandRevenueDistributionAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<DailyRevenueTableResponse>> GetDailyRevenueTableDataAsync(
        int days,
        CancellationToken cancellationToken);

    public Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(
        CancellationToken cancellationToken);

    public Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(CancellationToken cancellationToken);

    public Task<WorkshopOverviewResponse> GetWorkshopOverviewAsync(CancellationToken cancellationToken);

    public Task<FinancingOverviewResponse> GetFinancingOverviewAsync(CancellationToken cancellationToken);

    public Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(CancellationToken cancellationToken);

    public Task<CustomerServiceAnalyticsResponse> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken);
}
