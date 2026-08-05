using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPayrollSummaryForChat;

public class GetPayrollSummaryForChatQueryHandler(
    IEmployeeReadRepository employeeReadRepository,
    ICommissionReadRepository commissionReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetPayrollSummaryForChatQuery, Result<ChatToolEnvelope<ChatPayrollSummaryItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatPayrollSummaryItemDto>>> Handle(
        GetPayrollSummaryForChatQuery request,
        CancellationToken cancellationToken)
    {
        var today = dateProvider.VietnamToday;
        var month = request.Month is >= 1 and <= 12 ? request.Month.Value : today.Month;
        var year = request.Year is > 0 ? request.Year.Value : today.Year;
        var employees = await employeeReadRepository.GetAllWithUsersAsync(cancellationToken).ConfigureAwait(false);
        if (request.EmployeeId.HasValue)
        {
            employees = employees.Where(e => e.Id == request.EmployeeId.Value).ToList();
        }
        var allRecords = await commissionReadRepository.GetRecordsAsync(cancellationToken).ConfigureAwait(false);
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1);
        var period = $"{month:D2}/{year}";
        var allDtos = employees
            .Select(
                emp =>
                {
                    var periodRecords = allRecords
                        .Where(
                            r => r.EmployeeProfileId == emp.Id &&
                                    r.DateEarned >= periodStart &&
                                    r.DateEarned < periodEnd)
                        .ToList();
                    var totalCommission = periodRecords
                        .Where(r => r.Status is CommissionStatus.Confirmed or CommissionStatus.Paid)
                        .Sum(r => r.Amount);
                    return new ChatPayrollSummaryItemDto
                    {
                        EmployeeId = emp.Id,
                        FullName = emp.User?.FullName ?? "Unknown",
                        JobTitle = emp.JobTitle,
                        Period = period,
                        BaseSalary = emp.BaseSalary,
                        TotalCommission = totalCommission,
                        TotalNetPayable = emp.BaseSalary + totalCommission
                    };
                })
            .ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = allDtos.Take(limit).ToList();
        var inner = new ChatToolResult<ChatPayrollSummaryItemDto>(dtos, allDtos.Count, allDtos.Count > dtos.Count);
        var filtersApplied = new Dictionary<string, string> { ["Kỳ lương"] = period };
        if (request.EmployeeId.HasValue)
        {
            filtersApplied["Mã nhân viên"] = request.EmployeeId.Value.ToString();
        }
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IEmployeeReadRepository.GetAllWithUsersAsync+ICommissionReadRepository.GetRecordsAsync",
            filtersApplied,
            "luong-nhan-vien",
            "VND");
        return ChatToolEnvelope<ChatPayrollSummaryItemDto>.Wrap(inner, meta);
    }
}
