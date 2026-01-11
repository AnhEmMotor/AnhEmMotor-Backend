using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public sealed record GetDailyRevenueQuery(int Days = 7) : IRequest<Result<IEnumerable<DailyRevenueResponse>>>;
