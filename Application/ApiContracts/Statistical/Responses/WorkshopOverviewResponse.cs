using System;

namespace Application.ApiContracts.Statistical.Responses;

public class WorkshopOverviewResponse
{
    public WorkshopKpi Kpi { get; set; } = new();

    public List<WorkshopRepairOrderDto> RepairOrders { get; set; } = [];
}

public class WorkshopKpi
{
    public int InProgressCount { get; set; }

    public double AvgCompletionHours { get; set; }

    public decimal MonthlyRevenue { get; set; }

    public int OverdueCount { get; set; }
}

public class WorkshopRepairOrderDto
{
    public int Id { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string VehicleInfo { get; set; } = string.Empty;

    public string TechnicianName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? StartedAt { get; set; }

    public decimal LaborFee { get; set; }
}
