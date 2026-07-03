using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetEmployeeKPIs;

public class GetEmployeeKPIsQueryHandler(IEmployeeReadRepository employeeReadRepository)
    : IRequestHandler<GetEmployeeKPIsQuery, Result<List<KpiResponse>>>
{
    public async Task<Result<List<KpiResponse>>> Handle(
        GetEmployeeKPIsQuery request,
        CancellationToken cancellationToken)
    {
        var kpiEntities = await employeeReadRepository
            .GetAllWithKPIsAsync(cancellationToken)
            .ConfigureAwait(false);

        var response = kpiEntities.Select(k => new KpiResponse
        {
            EmployeeId = k.EmployeeProfileId,
            EmployeeName = k.EmployeeProfile?.User?.FullName ?? "Unknown",
            Period = $"{k.PeriodStart:dd/MM/yyyy} - {k.PeriodEnd:dd/MM/yyyy}",
            KpiName = k.MetricName,
            Target = k.TargetValue.ToString("N0"),
            Score = k.ActualValue >= k.TargetValue ? 100 : (int)Math.Round((k.ActualValue / k.TargetValue) * 100),
            EvaluatedAt = k.PeriodEnd
        }).ToList();

        return Result<List<KpiResponse>>.Success(response);
    }
}

public class KpiResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string KpiName { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime EvaluatedAt { get; set; }
}
