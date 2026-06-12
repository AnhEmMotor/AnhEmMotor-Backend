using System;
using System.Collections.Generic;

namespace Application.Api.Contracts.Statistical.Responses;

public class WorkshopDashboardResponse
{
    public KpiCards KpiCards { get; set; } = new();
    public UrgentAlerts Alerts { get; set; } = new();
    public Analytics Analytics { get; set; } = new();
    public Productivity Productivity { get; set; } = new();
}

public class KpiCards
{
    public int InProgressCount { get; set; }
    public double AvgCompletionHours { get; set; }
    public decimal CumulativeRevenue { get; set; }
}

public class UrgentAlerts
{
    public List<OverdueTicketDto> OverdueTickets { get; set; } = new();
    public List<PartShortageDto> PartShortages { get; set; } = new();
}

public class OverdueTicketDto
{
    public int TicketId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTimeOffset ExpectedCompletionTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PartShortageDto
{
    public int TicketId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}

public class Analytics
{
    public RevenueComparison RevenueComparison { get; set; } = new();
    public List<RevenueSourceDto> RevenueSources { get; set; } = new();
}

public class RevenueComparison
{
    public decimal WorkshopRevenue { get; set; }
    public decimal RetailRevenue { get; set; }
}

public class RevenueSourceDto
{
    public string Source { get; set; } = string.Empty; // "Labor", "Parts"
    public decimal Amount { get; set; }
}

public class Productivity
{
    public List<TechnicianStatusDto> TechnicianStatuses { get; set; } = new();
    public List<TechnicianRankingDto> TechnicianRankings { get; set; } = new();
}

public class TechnicianStatusDto
{
    public string TechnicianName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Busy", "Idle"
    public int? CurrentTicketId { get; set; }
}

public class TechnicianRankingDto
{
    public string TechnicianName { get; set; } = string.Empty;
    public int CompletedTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public double ComplaintRate { get; set; }
}
