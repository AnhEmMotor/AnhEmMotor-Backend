using Application.ApiContracts.Statistical.Responses;
using Application.Api.Contracts.Statistical.Responses;
namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalReadRepository
{
    Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken);

    Task<WorkshopDashboardResponse> GetWorkshopDashboardOverviewAsync(string from, string to, CancellationToken cancellationToken);

    Task<IEnumerable<DailyRevenueDetailResponse>> GetDailyRevenueDetailAsync(
    DateOnly reportDay,
    int days,
    CancellationToken cancellationToken);

    Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
    int months,
    CancellationToken cancellationToken);

    Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken);

    Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
    int variantId,
    CancellationToken cancellationToken);

    Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken);

    Task<IEnumerable<TopProductRevenueResponse>> GetTopProductsByRevenueAsync(
    int limit,
    CancellationToken cancellationToken);

    Task<IEnumerable<BrandRevenueResponse>> GetBrandRevenueDistributionAsync(CancellationToken cancellationToken);

    Task<IEnumerable<DailyRevenueTableResponse>> GetDailyRevenueTableDataAsync(
    int days,
    CancellationToken cancellationToken);

    Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(CancellationToken cancellationToken);

    Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(CancellationToken cancellationToken);

    Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(CancellationToken cancellationToken);

    Task<CustomerServiceAnalyticsResponse> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken);

 Task<IEnumerable<RevenueByCategoryResponse>> GetRevenueByCategoryAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);

 Task<IEnumerable<DailyCategoryRevenueResponse>> GetDailyCategoryRevenueAsync(int days, CancellationToken cancellationToken);
}
