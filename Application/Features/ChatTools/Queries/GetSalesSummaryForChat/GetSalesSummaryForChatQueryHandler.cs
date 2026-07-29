using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSalesSummaryForChat;

public class GetSalesSummaryForChatQueryHandler(IStatisticalReadRepository statisticalReadRepository)
    : IRequestHandler<GetSalesSummaryForChatQuery, Result<ChatToolResult<ChatDailyRevenueDto>>>
{
    public async Task<Result<ChatToolResult<ChatDailyRevenueDto>>> Handle(
        GetSalesSummaryForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate);
        var daily = await statisticalReadRepository.GetDailyRevenueAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var dailyList = daily.OrderByDescending(d => d.ReportDay).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = dailyList
            .Take(limit)
            .Select(d => new ChatDailyRevenueDto { ReportDay = d.ReportDay, TotalRevenue = d.TotalRevenue })
            .ToList();
        return new ChatToolResult<ChatDailyRevenueDto>(dtos, dailyList.Count, dailyList.Count > dtos.Count);
    }
}
