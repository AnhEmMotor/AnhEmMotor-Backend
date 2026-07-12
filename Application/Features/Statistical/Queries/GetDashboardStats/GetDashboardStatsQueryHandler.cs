using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsResponse>>
{
    public async Task<Result<DashboardStatsResponse>> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var _now = DateTimeOffset.UtcNow;
        var end = _now;
        var start = _now.AddDays(-30);
        var result = await repository.GetDashboardStatsAsync(start, end, cancellationToken).ConfigureAwait(false);
        return result ?? new DashboardStatsResponse();
    }
}
