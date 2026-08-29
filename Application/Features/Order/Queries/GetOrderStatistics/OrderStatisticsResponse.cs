namespace Application.Features.Order.Queries.GetOrderStatistics;

public class OrderStatisticsResponse
{
    public int PendingOrders { get; set; }

    public int SlaDelayed { get; set; }

    public int PaymentErrors { get; set; }

    public int ReturnRequests { get; set; }

    public int CompletedToday { get; set; }

    public int TargetToday { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal AverageOrderValue { get; set; }

    public double CancellationRate { get; set; }

    public List<HourlyOrderData> HourlyData { get; set; } = new();

    public List<DailyOrderData> DailyData { get; set; } = new();

    public List<OrderStatusStatData> StatusData { get; set; } = new();

    public List<DeliveryMethodStatData> DeliveryMethodData { get; set; } = new();

    public List<PaymentMethodStatData> PaymentMethodData { get; set; } = new();

    public List<ChannelStatData> ChannelData { get; set; } = new();

    public List<ExceptionOrder> ExceptionOrders { get; set; } = new();
}

public class HourlyOrderData
{
    public string Hour { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal Revenue { get; set; }
}

public class DailyOrderData
{
    public string Date { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal Revenue { get; set; }
}

public class OrderStatusStatData
{
    public string StatusId { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal TotalAmount { get; set; }
}

public class DeliveryMethodStatData
{
    public string Method { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}

public class PaymentMethodStatData
{
    public string Method { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal TotalAmount { get; set; }
}

public class ChannelStatData
{
    public string Channel { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal TotalAmount { get; set; }
}

public class ExceptionOrder
{
    public int Id { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public string StatusId { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public string Issue { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string WaitTime { get; set; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; set; }

    public string DeliveryType { get; set; } = string.Empty;
}

