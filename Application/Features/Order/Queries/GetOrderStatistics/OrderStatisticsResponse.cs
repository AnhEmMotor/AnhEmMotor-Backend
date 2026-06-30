namespace Application.Features.Order.Queries.GetOrderStatistics;

public class OrderStatisticsResponse
{
    public int PendingOrders { get; set; }
    public int SlaDelayed { get; set; }
    public int PaymentErrors { get; set; }
    public int ReturnRequests { get; set; }
    public int CompletedToday { get; set; }
    public int TargetToday { get; set; }
    public List<HourlyOrderData> HourlyData { get; set; } = new();
    public List<ExceptionOrder> ExceptionOrders { get; set; } = new();
}

public class HourlyOrderData
{
    public string Hour { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ExceptionOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
