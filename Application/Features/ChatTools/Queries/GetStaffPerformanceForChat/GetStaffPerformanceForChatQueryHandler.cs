using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetStaffPerformanceForChat;

public class GetStaffPerformanceForChatQueryHandler(
    IStatisticalAnalyticsRepository analyticsRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetStaffPerformanceForChatQuery, Result<ChatToolEnvelope<ChatStaffPerformanceItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatStaffPerformanceItemDto>>> Handle(
        GetStaffPerformanceForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var performance = await analyticsRepository
            .GetStaffPerformanceAsync(start.UtcDateTime, end.UtcDateTime, cancellationToken)
            .ConfigureAwait(false);
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = performance
            .Take(limit)
            .Select(
                p => new ChatStaffPerformanceItemDto
                {
                    EmployeeName = p.EmployeeName,
                    Role = p.Role,
                    TotalSales = p.TotalSales,
                    TargetSales = p.TargetSales,
                    KpiStatus = p.KpiStatus,
                    IsTopSeller = p.IsTopSeller
                })
            .ToList();
        var inner = new ChatToolResult<ChatStaffPerformanceItemDto>(
            dtos,
            performance.Count,
            performance.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalAnalyticsRepository.GetStaffPerformanceAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "hieu-suat-nhan-vien",
            null);
        return ChatToolEnvelope<ChatStaffPerformanceItemDto>.Wrap(inner, meta);
    }
}
