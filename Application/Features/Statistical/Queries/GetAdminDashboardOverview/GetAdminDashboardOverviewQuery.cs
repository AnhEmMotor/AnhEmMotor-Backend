using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminDashboardOverview;

public sealed record GetAdminDashboardOverviewQuery : IRequest<Result<AdminDashboardOverviewResponse>>
{
    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }
}
