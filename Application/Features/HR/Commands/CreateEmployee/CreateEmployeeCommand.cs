using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Domain.Entities.HR;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

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

public class CreateEmployeeCommandHandler(
    IUserCreateRepository userCreateRepository,
    IUserReadRepository userReadRepository,
    IEmployeeInsertRepository employeeInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var existingUser = await userReadRepository.FindUserByEmailAsync(request.Email, cancellationToken);
        Guid userId;

        if (existingUser == null)
        {
            // 2. Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true
            };

            var (succeeded, errors) = await userCreateRepository.CreateUserAsync(user, "DefaultPass123!", cancellationToken);
            if (!succeeded)
            {
                return Result<int>.Failure("Không thể tạo người dùng: " + string.Join(", ", errors));
            }
            userId = user.Id;
        }
        else
        {
            userId = existingUser.Id;
        }

        // 3. Create Employee Profile
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(employee.Id);
    }
}
