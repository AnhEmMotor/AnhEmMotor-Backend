using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed record GetOrderStatusCountsQuery : IRequest<IEnumerable<OrderStatusCountDto>>;
