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

public class UpdateEmployeeCommandHandler(
    IEmployeeReadRepository employeeReadRepository,
    IEmployeeUpdateRepository employeeUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee == null)
        {
            return Result<int>.Failure("Không tìm thấy hồ sơ nhân sự.");
        }
        employee.IdentityNumber = request.IdentityNumber.Trim();
        employee.Address = request.Address.Trim();
        employee.ContractDate = request.ContractDate;
        employee.BankName = request.BankName.Trim();
        employee.BankAccountNumber = request.BankAccountNumber.Trim();
        employee.JobTitle = request.JobTitle;
        employee.BaseSalary = request.BaseSalary;
        employeeUpdateRepository.Update(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(employee.Id);
    }
}
