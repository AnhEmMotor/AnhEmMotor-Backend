using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetStaffPerformance;

public class GetStaffPerformanceQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetStaffPerformanceQuery, List<StaffPerformanceResponse>>
{
    public Task<List<StaffPerformanceResponse>> Handle(
        GetStaffPerformanceQuery request,
        CancellationToken cancellationToken) => analyticsRepository.GetStaffPerformanceAsync(
        request.Start,
        request.End,
        cancellationToken);
}
