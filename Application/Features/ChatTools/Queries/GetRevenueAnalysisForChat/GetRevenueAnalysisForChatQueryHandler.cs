using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRevenueAnalysisForChat;

public class GetRevenueAnalysisForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetRevenueAnalysisForChatQuery, Result<ChatToolEnvelope<ChatRevenueAnalysisDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatRevenueAnalysisDto>>> Handle(
        GetRevenueAnalysisForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);

        var summary = await statisticalReadRepository.GetDashboardStatsAsync(start, end, cancellationToken)
            .ConfigureAwait(false) ?? new Application.ApiContracts.Statistical.Responses.DashboardStatsResponse();
        var dailyRevenue = await statisticalReadRepository.GetDailyRevenueAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var topProducts = await statisticalReadRepository.GetTopProductsByRevenueAsync(start, end, 5, cancellationToken)
            .ConfigureAwait(false);
        var brandDistribution = await statisticalReadRepository
            .GetBrandRevenueDistributionAsync(start, end, cancellationToken)
            .ConfigureAwait(false);

        var dto = new ChatRevenueAnalysisDto
        {
            TodayRevenue = summary.TodayRevenue,
            MonthlyRevenue = summary.MonthlyRevenue,
            TodayProfit = summary.TodayProfit,
            MonthlyProfit = summary.MonthlyProfit,
            RevenueTrend = dailyRevenue
                .Select(x => new ChatDailyRevenueItemDto { ReportDay = x.ReportDay, TotalRevenue = x.TotalRevenue })
                .ToList(),
            TopProducts = topProducts
                .Select(x => new ChatTopProductRevenueItemDto
                {
                    ProductName = x.ProductName,
                    UnitsSold = x.UnitsSold,
                    Revenue = x.Revenue
                })
                .ToList(),
            BrandRevenueDistribution = brandDistribution
                .Select(x => new ChatBrandRevenueItemDto
                {
                    BrandName = x.BrandName,
                    Revenue = x.Revenue,
                    QuantitySold = x.QuantitySold
                })
                .ToList()
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetDashboardStatsAsync",
            new Dictionary<string, string>
            {
                ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end)
            },
            "phan-tich-doanh-thu",
            "VND");

        return ChatToolEnvelope<ChatRevenueAnalysisDto>.WrapSingle(dto, meta);
    }
}
