using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Domain.Constants.HR.CommissionPolicy;
using Mapster;
using MediatR;
using System;
using System.Text.Json;

namespace Application.Features.HR.Commands.CreateCommissionPolicy
{
    public sealed class CreateCommissionPolicyCommandHandler(
        ICommissionPolicyRepository policyRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateCommissionPolicyCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(
            CreateCommissionPolicyCommand request,
            CancellationToken cancellationToken)
        {
            var existingPolicy = await policyRepository.GetExistingPolicyAsync(
                request.ProductId,
                request.CategoryId,
                request.EffectiveDate,
                cancellationToken).ConfigureAwait(false);

            if (existingPolicy != null)
            {
                return Result<int>.Failure(
                    Error.BadRequest(
                        "Định mức cho sản phẩm/nhóm này đã tồn tại trong khoảng thời gian hiệu lực (Yêu cầu cách nhau ít nhất 7 ngày)."));
            }

            var policy = request.Adapt<CommissionPolicy>();
            policyRepository.Add(policy);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var auditLog = new CommissionPolicyAuditLog
            {
                PolicyId = policy.Id,
                Action = CommissionPolicyAction.Created,
                ChangedByName = request.CurrentUserName,
                ChangedByUserId = request.CurrentUserId,
                NewValueSnapshot = JsonSerializer.Serialize(policy),
                Description =
                    $"Tạo định mức mới: {policy.Name} ({policy.Value}{(string.Compare(policy.Type, CommissionPolicyType.Percentage) == 0 ? "%" : "đ")})",
                ChangedAt = DateTime.UtcNow
            };

            policyRepository.AddAuditLog(auditLog);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return policy.Id;
        }
    }
}
