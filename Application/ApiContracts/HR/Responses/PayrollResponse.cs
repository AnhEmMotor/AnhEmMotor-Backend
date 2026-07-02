using System;

namespace Application.ApiContracts.HR.Responses;

public record PayrollResponse
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal ConfirmedCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public decimal TotalActualReceived => BaseSalary + ConfirmedCommission;
}

public record PayrollListItem
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Penalty { get; set; }
    public decimal TotalSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
}

public record PayrollStatistics
{
    public decimal TotalPayroll { get; set; }
    public decimal Paid { get; set; }
    public decimal Pending { get; set; }
    public int EmployeeCount { get; set; }
}
