using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminDashboardOverview;

public sealed class GetAdminDashboardOverviewQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminDashboardOverviewQuery, Result<AdminDashboardOverviewResponse>>
{
    public async Task<Result<AdminDashboardOverviewResponse>> Handle(
        GetAdminDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await repository.GetDashboardStatsAsync(cancellationToken).ConfigureAwait(false) ??
            new DashboardStatsResponse();
        var statusCounts = await repository.GetOrderStatusCountsAsync(cancellationToken).ConfigureAwait(false);
        var dailyRevenue = await repository.GetDailyRevenueAsync(7, cancellationToken).ConfigureAwait(false);
        var recentOrders = await repository.GetRecentOrdersAsync(5, cancellationToken).ConfigureAwait(false);

        return new AdminDashboardOverviewResponse
        {
            Summary = summary,
            OrderStatusDistribution = statusCounts,
            DailyRevenue = dailyRevenue,
            RecentOrders = recentOrders
        };
    }
}
