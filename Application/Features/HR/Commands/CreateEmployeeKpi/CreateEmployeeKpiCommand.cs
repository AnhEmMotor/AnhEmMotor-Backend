using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Commands.CreateEmployeeKpi;

public sealed record CreateEmployeeKpiCommand : IRequest<Result<int>>
{
    public int EmployeeProfileId { get; init; }

    public string MetricName { get; init; } = string.Empty;

    public decimal TargetValue { get; init; }

    public decimal ActualValue { get; init; }

    public DateTime PeriodStart { get; init; }

    public DateTime PeriodEnd { get; init; }

    public string? Description { get; init; }
}
