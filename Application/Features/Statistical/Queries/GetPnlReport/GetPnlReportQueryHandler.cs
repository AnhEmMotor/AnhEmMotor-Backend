using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetPnlReport;

public class GetPnlReportQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetPnlReportQuery, PnlReportResponse>
{
    public Task<PnlReportResponse> Handle(GetPnlReportQuery request, CancellationToken cancellationToken) => analyticsRepository.GetPnlReportAsync(
        request.Month,
        request.Year,
        cancellationToken);
}
