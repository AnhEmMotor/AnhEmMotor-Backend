namespace Application.ApiContracts.Statistical.Responses;

public class WorkshopReportResponse
{
	public WorkshopReportKpi Kpi { get; set; } = new();
	public List<WorkshopRepairOrderItem> RepairOrders { get; set; } = [];
}

public class WorkshopReportKpi
{
	public int InProgressCount { get; set; }
	public double AvgCompletionHours { get; set; }
	public decimal MonthlyRevenue { get; set; }
	public int OverdueCount { get; set; }
}

public class WorkshopRepairOrderItem
{
	public int Id { get; set; }
	public string OrderCode { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string VehicleInfo { get; set; } = string.Empty;
	public string? TechnicianName { get; set; }
	public string Status { get; set; } = string.Empty;
	public DateTimeOffset? StartedAt { get; set; }
	public decimal LaborFee { get; set; }
}
