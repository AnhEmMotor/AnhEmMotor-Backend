using Application.Api.Contracts.Statistical.Responses;
using Application.ApiContracts.Admin.Analytics;
using Application.ApiContracts.Statistical.Responses;

namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalReadRepository
{
    public Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<WorkshopDashboardResponse> GetWorkshopDashboardOverviewAsync(
        string from,
        string to,
        CancellationToken cancellationToken);

    public Task<IEnumerable<DailyRevenueDetailResponse>> GetDailyRevenueDetailAsync(
        DateOnly reportDay,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<DashboardStatsResponse?> GetDashboardStatsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken);

    public Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken);

    public Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(int variantId, CancellationToken cancellationToken);

    public Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken);

    public Task<IEnumerable<StaffPerformanceResponse>> GetTopStaffPerformanceAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int limit,
        CancellationToken cancellationToken);

    public Task<IEnumerable<TransactionLogResponse>> GetRecentTransactionsAsync(int limit, CancellationToken cancellationToken);

    public Task<IEnumerable<TopProductRevenueResponse>> GetTopProductsByRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int limit,
        CancellationToken cancellationToken);

    public Task<IEnumerable<BrandRevenueResponse>> GetBrandRevenueDistributionAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<IEnumerable<DailyRevenueTableResponse>> GetDailyRevenueTableDataAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(CancellationToken cancellationToken);

    public Task<CustomerServiceAnalyticsResponse> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<RevenueByCategoryResponse>> GetRevenueByCategoryAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<IEnumerable<DailyCategoryRevenueResponse>> GetDailyCategoryRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken);

    public Task<DashboardKpisResponse> GetDashboardKpisAsync(string period, CancellationToken cancellationToken);
}
