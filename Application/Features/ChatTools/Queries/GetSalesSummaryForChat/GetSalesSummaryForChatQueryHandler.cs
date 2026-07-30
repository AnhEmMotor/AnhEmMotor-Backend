using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSalesSummaryForChat;

public class GetSalesSummaryForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetSalesSummaryForChatQuery, Result<ChatToolEnvelope<ChatDailyRevenueDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatDailyRevenueDto>>> Handle(
        GetSalesSummaryForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var daily = await statisticalReadRepository.GetDailyRevenueAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var dailyList = daily.OrderByDescending(d => d.ReportDay).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = dailyList
            .Take(limit)
            .Select(d => new ChatDailyRevenueDto { ReportDay = d.ReportDay, TotalRevenue = d.TotalRevenue })
            .ToList();
        var inner = new ChatToolResult<ChatDailyRevenueDto>(dtos, dailyList.Count, dailyList.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetDailyRevenueAsync",
            new Dictionary<string, string>
            {
                ["Loại trừ"] = "Đơn huỷ, đơn nháp, bản ghi soft-delete",
                ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end)
            },
            "doanh-thu",
            "VND");
        return ChatToolEnvelope<ChatDailyRevenueDto>.Wrap(inner, meta);
    }
}
