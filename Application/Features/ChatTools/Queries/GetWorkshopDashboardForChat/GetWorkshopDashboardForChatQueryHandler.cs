using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWorkshopDashboardForChat;

public class GetWorkshopDashboardForChatQueryHandler(IStatisticalReadRepository repo, IServerDateProvider dateProvider) : IRequestHandler<GetWorkshopDashboardForChatQuery, Result<ChatToolEnvelope<ChatWorkshopDashboardDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatWorkshopDashboardDto>>> Handle(
        GetWorkshopDashboardForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var response = await repo
            .GetWorkshopDashboardOverviewAsync(
                start.ToString("yyyy-MM-dd"),
                end.ToString("yyyy-MM-dd"),
                cancellationToken)
            .ConfigureAwait(false);
        var dto = new ChatWorkshopDashboardDto
        {
            InProgressCount = response.KpiCards.InProgressCount,
            AvgCompletionHours = response.KpiCards.AvgCompletionHours,
            CumulativeRevenue = response.KpiCards.CumulativeRevenue,
            OverdueTicketsCount = response.Alerts.OverdueTickets.Count,
            PartShortagesCount = response.Alerts.PartShortages.Count,
            WarrantyRequestsCount = response.WarrantyRequestsCount,
            ComplaintsCount = response.ComplaintsCount
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetWorkshopDashboardOverviewAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "tong-quan-xuong-dich-vu",
            "VND");
        return ChatToolEnvelope<ChatWorkshopDashboardDto>.WrapSingle(dto, meta);
    }
}
