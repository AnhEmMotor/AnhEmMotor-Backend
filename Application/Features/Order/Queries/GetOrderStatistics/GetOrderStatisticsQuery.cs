using Application.Common.Models;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderStatistics;

public record GetOrderStatisticsQuery : IRequest<Result<OrderStatisticsResponse>>
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? Channel { get; init; }
    public string? PaymentMethod { get; init; }
    public string? StatusId { get; init; }
}

