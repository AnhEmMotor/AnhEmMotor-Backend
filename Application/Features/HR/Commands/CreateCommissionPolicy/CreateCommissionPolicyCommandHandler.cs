using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace Application.Features.HR.Commands.CreateCommissionPolicy
{
    public sealed class CreateCommissionPolicyCommandHandler(IApplicationDBContext context) : IRequestHandler<CreateCommissionPolicyCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(
            CreateCommissionPolicyCommand request,
            CancellationToken cancellationToken)
        {
            var startDate = request.EffectiveDate.AddDays(-7);
            var endDate = request.EffectiveDate.AddDays(7);
            var existingPolicy = await context.CommissionPolicies
                .Where(
                    p => p.IsActive &&
                        p.ProductId == request.ProductId &&
                        p.CategoryId == request.CategoryId &&
                        p.EffectiveDate > startDate &&
                        p.EffectiveDate < endDate)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (existingPolicy != null)
            {
                return Result<int>.Failure(
                    Error.BadRequest(
                        "Định mức cho sản phẩm/nhóm này đã tồn tại trong khoảng thời gian hiệu lực (Yêu cầu cách nhau ít nhất 7 ngày)."));
            }
            var policy = new CommissionPolicy
            {
                Name = request.Name,
                Type = request.Type,
                Value = request.Value,
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                TargetGroup = request.TargetGroup,
                EffectiveDate = request.EffectiveDate,
                Notes = request.Notes,
                Unit = request.Unit,
                IsActive = request.IsActive
            };
            context.CommissionPolicies.Add(policy);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var auditLog = new CommissionPolicyAuditLog
            {
                PolicyId = policy.Id,
                Action = "Created",
                ChangedByName = request.CurrentUserName,
                ChangedByUserId = request.CurrentUserId,
                NewValueSnapshot = JsonSerializer.Serialize(policy),
                Description =
                    $"Tạo định mức mới: {policy.Name} ({policy.Value}{(string.Compare(policy.Type, "Percentage") == 0 ? "%" : "đ")})",
                ChangedAt = DateTime.UtcNow
            };
            context.CommissionPolicyAuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return policy.Id;
        }
    }
}
