using Application.ApiContracts.Statistical.Responses;
using Application.Api.Contracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboardOverview;

public record GetWorkshopDashboardOverviewQuery(string? From, string? To)
    : IRequest<Result<WorkshopDashboardResponse>>;

public class GetWorkshopDashboardOverviewQueryHandler(
    IStatisticalReadRepository repository)
    : IRequestHandler<GetWorkshopDashboardOverviewQuery, Result<WorkshopDashboardResponse>>
{
    public async Task<Result<WorkshopDashboardResponse>> Handle(
        GetWorkshopDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var response = await repository.GetWorkshopDashboardOverviewAsync(
            request.From ?? "",
            request.To ?? "",
            cancellationToken).ConfigureAwait(false);
        return Result<WorkshopDashboardResponse>.Success(response);
    }
}
