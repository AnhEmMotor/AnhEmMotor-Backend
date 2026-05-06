using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Application.Features.HR.Commands.ApprovePayroll;

public record ApprovePayrollCommand(int? EmployeeId, int Month, int Year) : IRequest<Result>;

public sealed class ApprovePayrollCommandHandler(IApplicationDBContext context) : IRequestHandler<ApprovePayrollCommand, Result>
{
    public async Task<Result> Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
    {
        var query = context.CommissionRecords.Where(r => r.Status == CommissionStatus.Confirmed);
        if (request.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeProfileId == request.EmployeeId.Value);
        }
        var records = await query.ToListAsync(cancellationToken);
        if (records.Count == 0)
        {
            return Result.Failure("Không có khoản hoa hồng nào cần duyệt chi.");
        }
        foreach (var record in records)
        {
            record.Status = CommissionStatus.Paid;
            record.PaidAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
