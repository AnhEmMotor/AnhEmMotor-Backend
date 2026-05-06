using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public sealed class GetPayrollSummaryQueryHandler(IApplicationDBContext context) : IRequestHandler<GetPayrollSummaryQuery, Result<List<PayrollDTO>>>
{
    public async Task<Result<List<PayrollDTO>>> Handle(
        GetPayrollSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await context.EmployeeProfiles.Include(e => e.User).ToListAsync(cancellationToken);
        var result = new List<PayrollDTO>();
        foreach (var emp in employees)
        {
            var commissions = await context.CommissionRecords
                .Where(r => r.EmployeeProfileId == emp.Id)
                .ToListAsync(cancellationToken);
            result.Add(
                new PayrollDTO
                {
                    EmployeeId = emp.Id,
                    FullName = emp.User?.FullName ?? "Unknown",
                    JobTitle = emp.JobTitle,
                    BaseSalary = emp.BaseSalary,
                    PendingCommission = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.Amount),
                    ConfirmedCommission =
                        commissions.Where(c => c.Status == CommissionStatus.Confirmed).Sum(c => c.Amount),
                    PaidCommission = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.Amount)
                });
        }
        return result;
    }
}
