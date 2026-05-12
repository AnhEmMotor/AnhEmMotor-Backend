using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.HR.Commands.ApprovePayroll
{
    public sealed class ApprovePayrollCommandHandler(
        ICommissionReadRepository commissionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<ApprovePayrollCommand, Result>
    {
        public async Task<Result> Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
        {
            var records = await commissionRepository.GetRecordsByStatusAsync(
                CommissionStatus.Confirmed,
                request.EmployeeId,
                cancellationToken)
                .ConfigureAwait(false);
            if (records.Count == 0)
            {
                return Result.Failure("Không có khoản hoa hồng nào cần duyệt chi.");
            }
            foreach (var record in records)
            {
                record.Status = CommissionStatus.Paid;
                record.PaidAt = DateTime.UtcNow;
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
