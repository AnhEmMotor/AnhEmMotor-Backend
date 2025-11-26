namespace Application.Interfaces.Repositories.Statistical;

public class DailyRevenueDto
{
    public DateOnly ReportDay { get; set; }

    public long TotalRevenue { get; set; }
}

public class DashboardStatsDto
{
    public long LastMonthRevenue { get; set; }

    public long LastMonthProfit { get; set; }

    public long PendingOrdersCount { get; set; }

    public long NewCustomersCount { get; set; }
}

public class MonthlyRevenueProfitDto
{
    public DateOnly ReportMonth { get; set; }

    public long TotalRevenue { get; set; }

    public long TotalProfit { get; set; }
}

public class OrderStatusCountDto
{
    public string? StatusName { get; set; }

    public long OrderCount { get; set; }
}

public class ProductReportDto
{
    public string? ProductName { get; set; }

    public int VariantId { get; set; }

    public long StockQuantity { get; set; }

    public long SoldLastMonth { get; set; }
}

public class ProductStockPriceDto
{
    public long UnitPrice { get; set; }

    public long StockQuantity { get; set; }
}

public interface IStatisticalReadRepository
{
    Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(
        int days,
        CancellationToken cancellationToken);

    Task<DashboardStatsDto?> GetDashboardStatsAsync(
        CancellationToken cancellationToken);

    Task<IEnumerable<MonthlyRevenueProfitDto>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken);

    Task<IEnumerable<OrderStatusCountDto>> GetOrderStatusCountsAsync(
        CancellationToken cancellationToken);

    Task<IEnumerable<ProductReportDto>> GetProductReportLastMonthAsync(
        CancellationToken cancellationToken);

    Task<ProductStockPriceDto?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken);
}
