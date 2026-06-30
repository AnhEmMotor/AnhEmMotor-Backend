namespace Application.Features.SalesReports.Queries.GetSalesReport;

public class SalesReportResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int RefundedCount { get; set; }
    public int DeliveringCount { get; set; }
    public int DepositPaidCount { get; set; }
    public int WaitingDepositCount { get; set; }
    public List<SalesReportItem> Items { get; set; } = [];
}

public class SalesReportItem
{
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? StatusId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal Total { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? LastStatusChangedAt { get; set; }
}
