using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryResponse>>
{
    public async Task<Result<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await analyticsRepository.GetDashboardSummaryAsync(
            request.Start,
            request.End,
            cancellationToken);
        return result;
    }
}
