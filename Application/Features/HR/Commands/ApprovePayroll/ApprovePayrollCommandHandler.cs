using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.HR.Commands.ApprovePayroll;

public class ApprovePayrollCommandHandler(
    ICommissionReadRepository commissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ApprovePayrollCommand, Result>
{
    public async Task<Result> Handle(
        ApprovePayrollCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Month is < 1 or > 12 || request.Year < 1)
        {
            return Result.Failure("Ky luong khong hop le.");
        }

        var allRecords = await commissionRepository
            .GetRecordsByStatusAsync(
                CommissionStatus.Confirmed,
                request.EmployeeId,
                cancellationToken)
            .ConfigureAwait(false);

        var periodStart = new DateTime(request.Year, request.Month, 1);
        var periodEnd = periodStart.AddMonths(1);

        var periodRecords = allRecords
            .Where(r => r.DateEarned >= periodStart
                     && r.DateEarned < periodEnd)
            .ToList();

        if (periodRecords.Count == 0)
        {
            var monthLabel = $"{request.Month:00}/{request.Year}";
            return Result.Failure(
                $"Không có khoản hoa hồng nào cần duyệt chi cho tháng {monthLabel}.");
        }

        foreach (var record in periodRecords)
        {
            record.Status = CommissionStatus.Paid;
            record.PaidAt = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
