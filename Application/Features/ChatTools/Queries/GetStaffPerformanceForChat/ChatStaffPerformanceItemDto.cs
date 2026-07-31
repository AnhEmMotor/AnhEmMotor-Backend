namespace Application.Features.ChatTools.Queries.GetStaffPerformanceForChat;

public record ChatStaffPerformanceItemDto
{
    public string EmployeeName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public decimal TotalSales { get; init; }

    public decimal TargetSales { get; init; }

    public string KpiStatus { get; init; } = string.Empty;

    public bool IsTopSeller { get; init; }
}
