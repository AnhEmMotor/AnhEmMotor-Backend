using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderStatusTransitionMap;

public sealed class GetOrderStatusTransitionMapQueryHandler : IRequestHandler<GetOrderStatusTransitionMapQuery, Result<Dictionary<string, HashSet<string>>>>
{
    public Task<Result<Dictionary<string, HashSet<string>>>> Handle(
        GetOrderStatusTransitionMapQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = OrderStatusTransitions.GetAllowedTransitionsMap();
        return Task.FromResult(Result<Dictionary<string, HashSet<string>>>.Success(result));
    }
}
