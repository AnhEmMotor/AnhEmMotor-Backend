using Application.ApiContracts.Statistical.Responses;

namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalReadRepository
{
    public Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken);

    public Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken);

    public Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken);

    public Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken);
}
