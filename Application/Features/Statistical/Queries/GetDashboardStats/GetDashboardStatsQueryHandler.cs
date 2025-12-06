using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardStats;

public sealed class GetDashboardStatsQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    public async Task<DashboardStatsResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetDashboardStatsAsync(cancellationToken).ConfigureAwait(false);
        return result ?? new DashboardStatsResponse();
    }
}
