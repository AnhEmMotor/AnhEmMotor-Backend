namespace Application.Features.ChatTools.Queries.GetEmployeeKpiForChat;

public record ChatEmployeeKpiDto
{
    public int EmployeeId { get; init; }

    public string EmployeeName { get; init; } = string.Empty;

    public string Period { get; init; } = string.Empty;

    public string KpiName { get; init; } = string.Empty;

    public int Score { get; init; }
}
