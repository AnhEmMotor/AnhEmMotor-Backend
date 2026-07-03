using Application.Common.Models;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderStatistics;

public record GetOrderStatisticsQuery : IRequest<Result<OrderStatisticsResponse>>;
