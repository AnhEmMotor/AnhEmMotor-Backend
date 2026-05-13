using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.HR.Commands.CreateEmployee;

public record CreateEmployeeCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string IdentityNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public DateTime ContractDate { get; init; }

    public string BankName { get; init; } = string.Empty;

    public string BankAccountNumber { get; init; } = string.Empty;

    public string JobTitle { get; init; } = string.Empty;

    public decimal BaseSalary { get; init; }
}

