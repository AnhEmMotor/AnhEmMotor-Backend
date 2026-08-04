namespace Application.Features.ChatTools.Queries.GetPayrollSummaryForChat;

public record ChatPayrollSummaryItemDto
{
    public int EmployeeId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string JobTitle { get; init; } = string.Empty;

    /// <summary>
    /// Kỳ lương dạng "MM/yyyy".
    /// </summary>
    public string Period { get; init; } = string.Empty;

    public decimal BaseSalary { get; init; }

    public decimal TotalCommission { get; init; }

    public decimal TotalNetPayable { get; init; }
}
