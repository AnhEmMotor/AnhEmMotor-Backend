using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using MediatR;
using System;

namespace Application.Features.HR.Commands.UpdateEmployee;

public record UpdateEmployeeCommand : IRequest<Result<int>>
{
    public int Id { get; init; }

    public string IdentityNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public DateTime ContractDate { get; init; }

    public string BankName { get; init; } = string.Empty;

    public string BankAccountNumber { get; init; } = string.Empty;

    public string JobTitle { get; init; } = string.Empty;

    public decimal BaseSalary { get; init; }
}

