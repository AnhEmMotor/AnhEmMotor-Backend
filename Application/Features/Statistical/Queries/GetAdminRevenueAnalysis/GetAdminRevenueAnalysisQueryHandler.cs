using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;

public sealed class GetAdminRevenueAnalysisQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminRevenueAnalysisQuery, Result<AdminRevenueAnalysisResponse>>
{
    public async Task<Result<AdminRevenueAnalysisResponse>> Handle(
        GetAdminRevenueAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await repository.GetDashboardStatsAsync(cancellationToken).ConfigureAwait(false) ?? new DashboardStatsResponse();
        var dailyRevenue = await repository.GetDailyRevenueAsync(30, cancellationToken).ConfigureAwait(false);
        var tableData = await repository.GetDailyRevenueTableDataAsync(30, cancellationToken).ConfigureAwait(false);
        var topProducts = await repository.GetTopProductsByRevenueAsync(5, cancellationToken).ConfigureAwait(false);
        var brandDistribution = await repository.GetBrandRevenueDistributionAsync(cancellationToken).ConfigureAwait(false);

        // Giả lập phương thức thanh toán vì database hiện tại không lưu cột PaymentMethod
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
