using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public class GetPayrollSummaryQueryHandler(
    IEmployeeReadRepository employeeRepository,
    ICommissionReadRepository commissionRepository) : IRequestHandler<GetPayrollSummaryQuery, Result<List<PayrollResponse>>>
{
    private const int VolumeBonusOrderThreshold = 10;
    private const decimal VolumeBonusRate = 0.30m;

    public async Task<Result<List<PayrollResponse>>> Handle(
        GetPayrollSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month is < 1 or > 12 || request.Year < 1)
        {
            return Result<List<PayrollResponse>>.Failure("Ky luong khong hop le.");
        }
        var employees = await employeeRepository
            .GetAllWithUsersAsync(cancellationToken)
            .ConfigureAwait(false);
        var allRecords = await commissionRepository
            .GetRecordsAsync(cancellationToken)
            .ConfigureAwait(false);
        var periodStart = new DateTime(request.Year, request.Month, 1);
        var periodEnd = periodStart.AddMonths(1);
        var result = new List<PayrollResponse>();
        foreach (var emp in employees)
        {
            var periodRecords = allRecords
                .Where(r => r.EmployeeProfileId == emp.Id && r.DateEarned >= periodStart && r.DateEarned < periodEnd)
                .ToList();
            var pendingThisMonth = periodRecords
                .Where(r => r.Status == CommissionStatus.Pending)
                .Sum(r => r.Amount);
            var confirmedThisMonth = periodRecords
                .Where(r => r.Status == CommissionStatus.Confirmed)
                .Sum(r => r.Amount);
            var paidThisMonth = periodRecords
                .Where(r => r.Status == CommissionStatus.Paid)
                .Sum(r => r.Amount);
            var payrollEligibleRecords = periodRecords
                .Where(IsPayrollEligible)
                .ToList();
            var payrollEligibleOrderCount = payrollEligibleRecords
                .Select(r => r.OutputId)
                .Distinct()
                .Count();
            var payrollEligibleCommission = confirmedThisMonth + paidThisMonth;
            var volumeBonus = payrollEligibleOrderCount >= VolumeBonusOrderThreshold
                ? payrollEligibleCommission * VolumeBonusRate
                : 0;
            result.Add(
                new PayrollResponse
                {
                    EmployeeId = emp.Id,
                    FullName = emp.User?.FullName ?? "Unknown",
                    JobTitle = emp.JobTitle,
                    BaseSalary = emp.BaseSalary,
                    PendingCommission = pendingThisMonth,
                    ConfirmedCommission = confirmedThisMonth,
                    PaidCommission = paidThisMonth,
                    KpiBonus = volumeBonus,
                });
        }
        return Result<List<PayrollResponse>>.Success(result);
    }

    private static bool IsPayrollEligible(CommissionRecord record)
    {
        return record.Status is CommissionStatus.Confirmed or CommissionStatus.Paid;
    }
}
