using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public sealed record GetDailyRevenueQuery : IRequest<Result<IEnumerable<DailyRevenueResponse>>>
{
    public int Days { get; init; } = 7;
}
