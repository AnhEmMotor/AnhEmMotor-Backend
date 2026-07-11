using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminDashboardOverview;

public class GetAdminDashboardOverviewQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminDashboardOverviewQuery, Result<AdminDashboardOverviewResponse>>
{
    public async Task<Result<AdminDashboardOverviewResponse>> Handle(
        GetAdminDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var _now = DateTimeOffset.UtcNow;
        var end = request.EndDate ?? _now;
        var start = request.StartDate ?? _now.AddDays(-30);

        var summary = await repository.GetDashboardStatsAsync(start, end, cancellationToken).ConfigureAwait(false) ??
            new DashboardStatsResponse();
        var statusCounts = await repository.GetOrderStatusCountsAsync(cancellationToken).ConfigureAwait(false);
        var dailyRevenue = await repository.GetDailyRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        var recentOrders = await repository.GetRecentOrdersAsync(5, cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var currentMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        
        var topStaff = await repository.GetTopStaffPerformanceAsync(currentMonthStart, now, 5, cancellationToken).ConfigureAwait(false);
        var recentTransactions = await repository.GetRecentTransactionsAsync(10, cancellationToken).ConfigureAwait(false);

        return new AdminDashboardOverviewResponse
        {
            Summary = summary,
            OrderStatusDistribution = statusCounts,
            DailyRevenue = dailyRevenue,
            RecentOrders = recentOrders,
            TopStaff = topStaff,
            RecentTransactions = recentTransactions
        };
    }
}
