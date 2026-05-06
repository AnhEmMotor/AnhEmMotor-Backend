using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using System;
using System.Text.Json;

namespace Application.Features.HR.Commands.UpdateCommissionPolicy;

public record UpdateCommissionPolicyCommand : IRequest<Result>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "FixedAmount";

    public decimal Value { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public string? TargetGroup { get; set; }

    public DateTimeOffset EffectiveDate { get; set; }

    public string? Notes { get; set; }

    public string? Unit { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CurrentUserId { get; set; }

    public string CurrentUserName { get; set; } = "Admin";
}

public sealed class UpdateCommissionPolicyCommandHandler(IApplicationDBContext context) : IRequestHandler<UpdateCommissionPolicyCommand, Result>
{
    public async Task<Result> Handle(UpdateCommissionPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await context.CommissionPolicies.FindAsync(new object[] { request.Id }, cancellationToken);
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
        await context.SaveChangesAsync(cancellationToken);
        var auditLog = new CommissionPolicyAuditLog
        {
            PolicyId = policy.Id,
            Action = "Updated",
            ChangedByName = request.CurrentUserName,
            ChangedByUserId = request.CurrentUserId,
            OldValueSnapshot = oldSnapshot,
            NewValueSnapshot = JsonSerializer.Serialize(policy),
            Description =
                $"Cập nhật định mức: {policy.Name}. Giá trị mới: {policy.Value}{(policy.Type == "Percentage" ? "%" : "đ")}",
            ChangedAt = DateTime.UtcNow
        };
        context.CommissionPolicyAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
