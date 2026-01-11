using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery : IRequest<Result<DashboardStatsResponse>>;
