using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderCancellableStatuses;

public sealed class GetOrderCancellableStatusesQueryHandler : IRequestHandler<GetOrderCancellableStatusesQuery, Result<IEnumerable<string>>>
{
    public Task<Result<IEnumerable<string>>> Handle(
        GetOrderCancellableStatusesQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var result = OrderStatusTransitions.GetAllowedTransitionsMap()
            .Where(t => t.Value.Contains(OrderStatus.Cancelled))
            .Select(t => t.Key);
        
        return Task.FromResult(Result<IEnumerable<string>>.Success(result));
    }
}
