using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetPnlReport;

public class GetPnlReportQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetPnlReportQuery, Result<PnlReportResponse>>
{
    public async Task<Result<PnlReportResponse>> Handle(GetPnlReportQuery request, CancellationToken cancellationToken)
    {
        var result = await analyticsRepository.GetPnlReportAsync(
            request.Month,
            request.Year,
            cancellationToken);
        return result;
    }
}
