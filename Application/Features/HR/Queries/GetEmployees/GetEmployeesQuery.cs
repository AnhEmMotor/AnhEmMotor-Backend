using Application.Common.Models;
using Application.Interfaces.Repositories.HR;
using MediatR;
using System.Linq;

namespace Application.Features.HR.Queries.GetEmployees;

public record GetEmployeesQuery : IRequest<Result<List<EmployeeDTO>>>;

public class GetEmployeesQueryHandler(IEmployeeReadRepository employeeReadRepository) : IRequestHandler<GetEmployeesQuery, Result<List<EmployeeDTO>>>
{
    public async Task<Result<List<EmployeeDTO>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await employeeReadRepository.GetAllWithUsersAsync(cancellationToken);
        var dtos = employees.Select(
            e => new EmployeeDTO
            {
                Id = e.Id,
                UserId = e.UserId,
                FullName = e.User.FullName,
                Email = e.User.Email ?? string.Empty,
                JobTitle = e.JobTitle,
                BaseSalary = e.BaseSalary,
                IdentityNumber = e.IdentityNumber,
                Address = e.Address,
                ContractDate = e.ContractDate,
                BankName = e.BankName,
                BankAccountNumber = e.BankAccountNumber,
                AvatarUrl = e.User.AvatarUrl
            })
            .ToList();
        return Result<List<EmployeeDTO>>.Success(dtos);
    }
}
