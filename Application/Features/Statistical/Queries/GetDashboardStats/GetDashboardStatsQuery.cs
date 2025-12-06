using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery : IRequest<DashboardStatsResponse>;
