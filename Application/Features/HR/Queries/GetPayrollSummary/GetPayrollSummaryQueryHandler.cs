using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public sealed class GetPayrollSummaryQueryHandler(
    IEmployeeReadRepository employeeRepository,
    ICommissionReadRepository commissionRepository) : IRequestHandler<GetPayrollSummaryQuery, Result<List<PayrollResponse>>>
{
    public async Task<Result<List<PayrollResponse>>> Handle(
        GetPayrollSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetAllWithUsersAsync(cancellationToken).ConfigureAwait(false);
        var result = new List<PayrollResponse>();

        foreach (var emp in employees)
        {
            var commissions = await commissionRepository.GetRecordsByEmployeeIdAsync(emp.Id, cancellationToken).ConfigureAwait(false);
            
            result.Add(
                new PayrollResponse
                {
                    EmployeeId = emp.Id,
                    FullName = emp.User?.FullName ?? "Unknown",
                    JobTitle = emp.JobTitle,
                    BaseSalary = emp.BaseSalary,
                    PendingCommission = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.Amount),
                    ConfirmedCommission = commissions.Where(c => c.Status == CommissionStatus.Confirmed).Sum(c => c.Amount),
                    PaidCommission = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.Amount)
                });
        }

        return Result<List<PayrollResponse>>.Success(result);
    }
}

