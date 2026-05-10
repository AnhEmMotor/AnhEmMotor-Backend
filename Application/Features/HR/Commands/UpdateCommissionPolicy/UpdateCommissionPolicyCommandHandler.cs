using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Domain.Constants.HR.CommissionPolicy;
using Mapster;
using MediatR;
using System;
using System.Text.Json;

namespace Application.Features.HR.Commands.UpdateCommissionPolicy
{
    public sealed class UpdateCommissionPolicyCommandHandler(
        ICommissionPolicyRepository policyRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateCommissionPolicyCommand, Result>
    {
        public async Task<Result> Handle(UpdateCommissionPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await policyRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (policy == null)
                return Result.Failure("Không tìm thấy chính sách.");

            var oldSnapshot = JsonSerializer.Serialize(policy);
            request.Adapt(policy);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var auditLog = new CommissionPolicyAuditLog
            {
                PolicyId = policy.Id,
                Action = CommissionPolicyAction.Updated,
                ChangedByName = request.CurrentUserName,
                ChangedByUserId = request.CurrentUserId,
                OldValueSnapshot = oldSnapshot,
                NewValueSnapshot = JsonSerializer.Serialize(policy),
                Description =
                    $"Cập nhật định mức: {policy.Name}. Giá trị mới: {policy.Value}{(string.Compare(policy.Type, CommissionPolicyType.Percentage) == 0 ? "%" : "đ")}",
                ChangedAt = DateTime.UtcNow
            };

            policyRepository.AddAuditLog(auditLog);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
