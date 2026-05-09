using Application.Common.Models;
using MediatR;
using System;

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

