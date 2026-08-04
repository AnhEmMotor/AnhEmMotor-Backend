using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Kpi;
using MediatR;

namespace Application.Features.HR.Queries.GetEmployeeKPIs;

public class GetEmployeeKPIsQueryHandler(IEmployeeKpiRepository kpiRepository) : IRequestHandler<GetEmployeeKPIsQuery, Result<List<KpiResponse>>>
{
    public async Task<Result<List<KpiResponse>>> Handle(
        GetEmployeeKPIsQuery request,
        CancellationToken cancellationToken)
    {
        var kpiEntities = await kpiRepository
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);
        var response = kpiEntities.Select(
            k => new KpiResponse
            {
                Id = k.Id,
                EmployeeId = k.EmployeeProfileId,
                EmployeeName = k.EmployeeProfile?.User?.FullName ?? "Unknown",
                JobTitle = k.EmployeeProfile?.JobTitle ?? string.Empty,
                Period = $"{k.PeriodStart:dd/MM/yyyy} - {k.PeriodEnd:dd/MM/yyyy}",
                KpiName = k.MetricName,
                Target = k.TargetValue.ToString("N0"),
                TargetValue = k.TargetValue,
                ActualValue = k.ActualValue,
                Score =
                    k.TargetValue <= 0
                            ? 0
                            : Math.Round((k.ActualValue / k.TargetValue) * 100, 1, MidpointRounding.AwayFromZero),
                PeriodStart = k.PeriodStart,
                PeriodEnd = k.PeriodEnd,
                EvaluatedAt = k.PeriodEnd,
                Description = k.Description
            })
            .ToList();
        return Result<List<KpiResponse>>.Success(response);
    }
}

public class KpiResponse
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Period { get; set; } = string.Empty;

    public string KpiName { get; set; } = string.Empty;

    public string Target { get; set; } = string.Empty;

    public decimal TargetValue { get; set; }

    public decimal ActualValue { get; set; }

    public decimal Score { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public string? Description { get; set; }
}
