using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    public Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken) => analyticsRepository.GetDashboardSummaryAsync(
        request.Start,
        request.End,
        cancellationToken);
}
