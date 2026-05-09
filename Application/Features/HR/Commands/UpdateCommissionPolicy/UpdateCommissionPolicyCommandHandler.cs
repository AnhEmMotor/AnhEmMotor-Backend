using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Application.Features.HR.Commands.UpdateCommissionPolicy
{
    public sealed class UpdateCommissionPolicyCommandHandler(IApplicationDBContext context) : IRequestHandler<UpdateCommissionPolicyCommand, Result>
    {
        public async Task<Result> Handle(UpdateCommissionPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await context.CommissionPolicies.FindAsync([request.Id], cancellationToken).ConfigureAwait(false);
            if (policy == null)
                return Result.Failure("Không tìm thấy chính sách.");
            var oldSnapshot = JsonSerializer.Serialize(policy);
            policy.Name = request.Name;
            policy.Type = request.Type;
            policy.Value = request.Value;
            policy.ProductId = request.ProductId;
            policy.CategoryId = request.CategoryId;
            policy.TargetGroup = request.TargetGroup;
            policy.EffectiveDate = request.EffectiveDate;
            policy.Notes = request.Notes;
            policy.Unit = request.Unit;
            policy.IsActive = request.IsActive;
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var auditLog = new CommissionPolicyAuditLog
            {
                PolicyId = policy.Id,
                Action = "Updated",
                ChangedByName = request.CurrentUserName,
                ChangedByUserId = request.CurrentUserId,
                OldValueSnapshot = oldSnapshot,
                NewValueSnapshot = JsonSerializer.Serialize(policy),
                Description =
                    $"Cập nhật định mức: {policy.Name}. Giá trị mới: {policy.Value}{(string.Compare(policy.Type, "Percentage") == 0 ? "%" : "đ")}",
                ChangedAt = DateTime.UtcNow
            };
            context.CommissionPolicyAuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }

}
