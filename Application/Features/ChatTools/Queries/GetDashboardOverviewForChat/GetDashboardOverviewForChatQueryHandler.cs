using Application.ApiContracts.Statistical.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetDashboardOverviewForChat;

public class GetDashboardOverviewForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetDashboardOverviewForChatQuery, Result<ChatToolEnvelope<ChatDashboardOverviewDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatDashboardOverviewDto>>> Handle(
        GetDashboardOverviewForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var stats = await statisticalReadRepository.GetDashboardStatsAsync(start, end, cancellationToken)
                .ConfigureAwait(false) ??
            new DashboardStatsResponse();
        var dto = new ChatDashboardOverviewDto
        {
            TodayRevenue = stats.TodayRevenue,
            MonthlyRevenue = stats.MonthlyRevenue,
            TodayProfit = stats.TodayProfit,
            MonthlyProfit = stats.MonthlyProfit,
            PendingOrdersCount = stats.PendingOrdersCount,
            OverdueOrdersCount = stats.OverdueOrdersCount,
            NewCustomersCount = stats.NewCustomersCount,
            TodayVehiclesSold = stats.TodayVehiclesSold,
            MonthlyVehiclesSold = stats.MonthlyVehiclesSold,
            CurrentInventoryCount = stats.CurrentInventoryCount,
            OverdueDebtAmount = stats.OverdueDebtAmount
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetDashboardStatsAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "tong-quan-dashboard",
            null);
        return ChatToolEnvelope<ChatDashboardOverviewDto>.WrapSingle(dto, meta);
    }
}
