using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public sealed record GetDailyRevenueQuery(int Days = 7) : IRequest<IEnumerable<DailyRevenueDto>>;
