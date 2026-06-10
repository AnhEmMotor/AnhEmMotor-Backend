using System;
using System.Collections.Generic;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class LogisticsDashboardResponse

{
    public LogisticsDashboardSummary Summary { get; set; } = new();

    public Dictionary<string, int> FulfillmentFunnel { get; set; } = new(); // status -> count

    public List<LogisticsTrendPoint> Trends { get; set; } = new(); // bar+line

    public List<CarrierScoreRow> CarrierScorecard { get; set; } = new();

    public List<LogisticsExceptionRow> Exceptions { get; set; } = new();
}

public class LogisticsDashboardSummary
{
    public int FulfillmentWorkload { get; set; }
    public decimal PendingUnreconciledCod { get; set; }
    public double OtifRate { get; set; }
    public double ReturnsClaimsRate { get; set; }
    public bool FulfillmentWorkloadIsOverload { get; set; }
}

public class LogisticsTrendPoint
{
    public string DayLabel { get; set; } = string.Empty;
    public int DeliveredCount { get; set; }
    public decimal ShippingCost { get; set; }
}

public class CarrierScoreRow
{
    public string Carrier { get; set; } = string.Empty;
    public int DeliveredCount { get; set; }
    public double AvgDeliveryDays { get; set; }
    public decimal AvgShippingCostPerOrder { get; set; }
    public double ReturnsRatio { get; set; }
}

public class LogisticsExceptionRow
{
    public string Type { get; set; } = string.Empty; // ngâm kho / giao chậm / hoàn chờ kiểm tra
    public string TrackingNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

