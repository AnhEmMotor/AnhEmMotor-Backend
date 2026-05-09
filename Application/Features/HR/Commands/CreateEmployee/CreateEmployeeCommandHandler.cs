using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Domain.Entities.HR;
using MediatR;
using System;

namespace Application.Features.HR.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler(
        IUserCreateRepository userCreateRepository,
        IUserReadRepository userReadRepository,
        IEmployeeInsertRepository employeeInsertRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await userReadRepository.FindUserByEmailAsync(request.Email, cancellationToken)
                .ConfigureAwait(false);
            Guid userId;
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    EmailConfirmed = true
                };
                var (succeeded, errors) = await userCreateRepository.CreateUserAsync(
                    user,
                    "DefaultPass123!",
                    cancellationToken)
                    .ConfigureAwait(false);
                if (!succeeded)
                {
                    return Result<int>.Failure($"Không thể tạo người dùng: {string.Join(", ", errors)}");
                }
                userId = user.Id;
            } else
            {
                userId = existingUser.Id;
            }
            var employee = new EmployeeProfile
            {
                UserId = userId,
                IdentityNumber = request.IdentityNumber,
                Address = request.Address,
                ContractDate = request.ContractDate,
                BankName = request.BankName,
                BankAccountNumber = request.BankAccountNumber,
                JobTitle = request.JobTitle,
                BaseSalary = request.BaseSalary
            };
            employeeInsertRepository.Insert(employee);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(employee.Id);
        }
    }
}
