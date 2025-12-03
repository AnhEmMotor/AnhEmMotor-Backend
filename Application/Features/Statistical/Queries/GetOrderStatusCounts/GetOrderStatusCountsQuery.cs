using Application.ApiContracts.Staticals;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed record GetOrderStatusCountsQuery : IRequest<IEnumerable<OrderStatusCountResponse>>;
