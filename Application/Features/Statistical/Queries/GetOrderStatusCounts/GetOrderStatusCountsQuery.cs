using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed record GetOrderStatusCountsQuery : IRequest<IEnumerable<OrderStatusCountResponse>>;
