using Application.ApiContracts.Staticals;

namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalReadRepository
{
    Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken);

    Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken);

    Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken);

    Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(int variantId, CancellationToken cancellationToken);
}
