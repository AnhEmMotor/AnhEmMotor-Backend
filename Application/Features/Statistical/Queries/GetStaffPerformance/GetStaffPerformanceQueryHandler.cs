using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetStaffPerformance;

public class GetStaffPerformanceQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetStaffPerformanceQuery, Result<List<StaffPerformanceResponse>>>
{
    public async Task<Result<List<StaffPerformanceResponse>>> Handle(
        GetStaffPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var result = await analyticsRepository.GetStaffPerformanceAsync(
            request.Start,
            request.End,
            cancellationToken);
        return result;
    }
}
