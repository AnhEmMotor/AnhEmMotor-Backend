using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.HR.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdQueryHandler(IEmployeeReadRepository employeeReadRepository)
    : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeResponse>>
{
    public async Task<Result<EmployeeResponse>> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (employee is null)
        {
            return Result<EmployeeResponse>.Failure("Không tìm thấy hồ sơ nhân viên.");
        }

        return Result<EmployeeResponse>.Success(new EmployeeResponse
        {
            Id = employee.Id,
            UserId = employee.UserId,
            FullName = employee.User.FullName,
            Email = employee.User.Email ?? string.Empty,
            JobTitle = employee.JobTitle,
            BaseSalary = employee.BaseSalary,
            IdentityNumber = employee.IdentityNumber,
            Address = employee.Address,
            ContractDate = employee.ContractDate,
            BankName = employee.BankName,
            BankAccountNumber = employee.BankAccountNumber,
            AvatarUrl = employee.User.AvatarUrl
        });
    }
}
