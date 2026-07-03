using System;
using System.Collections.Generic;

namespace Application.Features.Statistical.DTOs;

public class WorkshopDashboardDto
{
    public SummaryCardsDto SummaryCards { get; set; } = new();
    public FinancialSummaryDto FinancialSummary { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
    public List<TopServiceDto> TopServices { get; set; } = new();
    public List<StatusBreakdownDto> StatusBreakdowns { get; set; } = new();
}

public class SummaryCardsDto
{
    public int TotalBookings { get; set; }
    public int TotalRepairOrders { get; set; }
    public int TotalMaintenances { get; set; }
    public int TotalWarrantyClaims { get; set; }
    public double AvgCompletionHours { get; set; }
}

public class FinancialSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalUnpaidAmount { get; set; }
    public decimal TotalPartialAmount { get; set; }
    public int UnpaidInvoicesCount { get; set; }
}

public class DailyRevenueDto
{
    public DateTime RevenueDate { get; set; }
    public decimal DailyRevenue { get; set; }
}

public class TopServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalServiceRevenue { get; set; }
}

public class StatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;
    public int StatusCount { get; set; }
}
