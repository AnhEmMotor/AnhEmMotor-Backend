using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.User;
using MediatR;
using System;

namespace Application.Features.HR.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler(
        IEmployeeReadRepository employeeReadRepository,
        IEmployeeUpdateRepository employeeUpdateRepository,
        IUserUpdateRepository userUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await employeeReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (employee == null)
            {
                return Result<int>.Failure("Không tìm thấy hồ sơ nhân sự.");
            }
            employee.User.FullName = request.FullName.Trim();
            employee.User.Email = request.Email.Trim();
            employee.User.UserName = request.Email.Trim();
            employee.IdentityNumber = request.IdentityNumber.Trim();
            employee.Address = request.Address.Trim();
            employee.ContractDate = DateTime.SpecifyKind(request.ContractDate, DateTimeKind.Utc);
            employee.BankName = request.BankName.Trim();
            employee.BankAccountNumber = request.BankAccountNumber.Trim();
            employee.JobTitle = request.JobTitle.Trim();
            employee.BaseSalary = request.BaseSalary;
            employeeUpdateRepository.Update(employee);
            var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(employee.User, cancellationToken)
                .ConfigureAwait(false);
            if (!succeeded)
            {
                return Result<int>.Failure($"Không thể cập nhật tài khoản nhân viên: {string.Join(", ", errors)}");
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(employee.Id);
        }
    }
}
