using System;

namespace Application.Features.HR.Queries.GetEmployees;

public record EmployeeDTO
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public decimal BaseSalary { get; set; }

    public string IdentityNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime ContractDate { get; set; }

    public string BankName { get; set; } = string.Empty;

    public string BankAccountNumber { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }
}
