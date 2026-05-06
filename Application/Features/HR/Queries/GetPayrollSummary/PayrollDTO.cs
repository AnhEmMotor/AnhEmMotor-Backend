using System;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public record PayrollDTO
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
