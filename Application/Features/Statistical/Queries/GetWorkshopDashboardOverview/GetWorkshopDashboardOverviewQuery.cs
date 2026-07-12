using Application.Api.Contracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboardOverview;

public record GetWorkshopDashboardOverviewQuery(string? From, string? To) : IRequest<Result<WorkshopDashboardResponse>>;

public class GetWorkshopDashboardOverviewQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetWorkshopDashboardOverviewQuery, Result<WorkshopDashboardResponse>>
{
    public async Task<Result<WorkshopDashboardResponse>> Handle(
        GetWorkshopDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var response = await repository.GetWorkshopDashboardOverviewAsync(
            request.From ?? string.Empty,
            request.To ?? string.Empty,
            cancellationToken)
            .ConfigureAwait(false);
        return Result<WorkshopDashboardResponse>.Success(response);
    }
}
