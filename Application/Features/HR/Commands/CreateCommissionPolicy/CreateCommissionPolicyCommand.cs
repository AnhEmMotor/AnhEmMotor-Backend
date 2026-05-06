using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities.HR;
using MediatR;
using System;
using System.Text.Json;

namespace Application.Features.HR.Commands.CreateCommissionPolicy;

public record CreateCommissionPolicyCommand : IRequest<Result<int>>
{
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

public sealed class CreateCommissionPolicyCommandHandler(IApplicationDBContext context) : IRequestHandler<CreateCommissionPolicyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateCommissionPolicyCommand request, CancellationToken cancellationToken)
    {
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
        await context.SaveChangesAsync(cancellationToken);
        var auditLog = new CommissionPolicyAuditLog
        {
            PolicyId = policy.Id,
            Action = "Created",
            ChangedByName = request.CurrentUserName,
            ChangedByUserId = request.CurrentUserId,
            NewValueSnapshot = JsonSerializer.Serialize(policy),
            Description = $"Tạo định mức mới: {policy.Name} ({policy.Value}{(policy.Type == "Percentage" ? "%" : "đ")})",
            ChangedAt = DateTime.UtcNow
        };
        context.CommissionPolicyAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
        return policy.Id;
    }
}
