using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;

public class GetAdminRevenueAnalysisQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminRevenueAnalysisQuery, Result<AdminRevenueAnalysisResponse>>
{
    public async Task<Result<AdminRevenueAnalysisResponse>> Handle(
        GetAdminRevenueAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var _now = DateTimeOffset.UtcNow;
        var end = request.EndDate ?? _now;
        var start = request.StartDate ?? _now.AddDays(-30);

        var summary = await repository.GetDashboardStatsAsync(start, end, cancellationToken).ConfigureAwait(false) ??
            new DashboardStatsResponse();
        var dailyRevenue = await repository.GetDailyRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        var tableData = await repository.GetDailyRevenueTableDataAsync(start, end, cancellationToken).ConfigureAwait(false);
        var topProducts = await repository.GetTopProductsByRevenueAsync(start, end, 5, cancellationToken).ConfigureAwait(false);
        var brandDistribution = await repository.GetBrandRevenueDistributionAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var paymentMethods = new List<PaymentMethodDistributionResponse>
        {
            new PaymentMethodDistributionResponse { MethodName = "Tiền mặt", Value = 65 },
            new PaymentMethodDistributionResponse { MethodName = "Chuyển khoản", Value = 35 }
        };
        return new AdminRevenueAnalysisResponse
        {
            Summary = summary,
            RevenueTrend = dailyRevenue,
            TopProductsByRevenue = topProducts,
            BrandRevenueDistribution = brandDistribution,
            PaymentMethodDistribution = paymentMethods,
            DailyTableData = tableData
        };
    }
}
