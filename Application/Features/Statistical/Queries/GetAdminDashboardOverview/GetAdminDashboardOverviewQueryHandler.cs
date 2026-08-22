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
        var vnTz = TimeSpan.FromHours(7);
        var _now = DateTimeOffset.UtcNow.ToOffset(vnTz);
        var currentMonthStart = new DateTimeOffset(_now.Year, _now.Month, 1, 0, 0, 0, vnTz);

        var start = request.StartDate.HasValue
            ? new DateTimeOffset(request.StartDate.Value.Year, request.StartDate.Value.Month, request.StartDate.Value.Day, 0, 0, 0, vnTz)
            : currentMonthStart;
        var end = request.EndDate.HasValue
            ? new DateTimeOffset(request.EndDate.Value.Year, request.EndDate.Value.Month, request.EndDate.Value.Day, 23, 59, 59, 999, vnTz)
            : _now;

        var summary = await repository.GetDashboardStatsAsync(start, end, cancellationToken).ConfigureAwait(false) ??
            new DashboardStatsResponse();
        var statusCounts = await repository.GetOrderStatusCountsAsync(cancellationToken).ConfigureAwait(false);
        var dailyRevenue = await repository.GetDailyRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        var recentOrders = await repository.GetRecentOrdersAsync(10, cancellationToken).ConfigureAwait(false);
        var topStaff = await repository.GetTopStaffPerformanceAsync(start, end, 5, cancellationToken)
            .ConfigureAwait(false);
        var recentTransactions = await repository.GetRecentTransactionsAsync(10, cancellationToken)
            .ConfigureAwait(false);
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
