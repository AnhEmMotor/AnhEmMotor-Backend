using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;
using System;

namespace Application.Features.HR.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler(
        IEmployeeReadRepository employeeReadRepository,
        IEmployeeUpdateRepository employeeUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await employeeReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (employee == null)
            {
                return Result<int>.Failure("Không tìm th?y h? so nhân s?.");
            }
            employee.IdentityNumber = request.IdentityNumber.Trim();
            employee.Address = request.Address.Trim();
            employee.ContractDate = request.ContractDate;
            employee.BankName = request.BankName.Trim();
            employee.BankAccountNumber = request.BankAccountNumber.Trim();
            employee.JobTitle = request.JobTitle;
            employee.BaseSalary = request.BaseSalary;
            employeeUpdateRepository.Update(employee);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(employee.Id);
        }
    }
}
