using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetEmployeeKpiForChat;

public class GetEmployeeKpiForChatQueryHandler(
    IEmployeeReadRepository employeeReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetEmployeeKpiForChatQuery, Result<ChatToolEnvelope<ChatEmployeeKpiDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatEmployeeKpiDto>>> Handle(
        GetEmployeeKpiForChatQuery request,
        CancellationToken cancellationToken)
    {
        var kpiEntities = await employeeReadRepository
            .GetAllWithKPIsAsync(cancellationToken)
            .ConfigureAwait(false);
        var employeeKpis = kpiEntities
            .Where(k => k.EmployeeProfileId == request.EmployeeId)
            .OrderByDescending(k => k.PeriodEnd)
            .ToList();
        var dtos = employeeKpis
            .Select(
                k => new ChatEmployeeKpiDto
                {
                    EmployeeId = k.EmployeeProfileId,
                    EmployeeName = k.EmployeeProfile?.User?.FullName ?? "Unknown",
                    Period = $"{k.PeriodStart:dd/MM/yyyy} - {k.PeriodEnd:dd/MM/yyyy}",
                    KpiName = k.MetricName,
                    Score = k.ActualValue >= k.TargetValue ? 100 : (int)Math.Round(k.ActualValue / k.TargetValue * 100)
                })
            .ToList();
        var inner = new ChatToolResult<ChatEmployeeKpiDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IEmployeeReadRepository.GetAllWithKPIsAsync",
            new Dictionary<string, string> { ["Mã nhân viên"] = request.EmployeeId.ToString() },
            "kpi-nhan-vien",
            null);
        return ChatToolEnvelope<ChatEmployeeKpiDto>.Wrap(inner, meta);
    }
}
