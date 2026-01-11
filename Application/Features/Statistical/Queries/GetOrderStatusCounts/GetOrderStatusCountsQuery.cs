using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed record GetOrderStatusCountsQuery : IRequest<Result<IEnumerable<OrderStatusCountResponse>>>;
