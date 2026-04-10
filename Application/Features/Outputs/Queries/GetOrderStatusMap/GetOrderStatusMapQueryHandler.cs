using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderStatusMap;

public sealed class GetOrderStatusMapQueryHandler : IRequestHandler<GetOrderStatusMapQuery, Result<IEnumerable<object>>>
{
    public Task<Result<IEnumerable<object>>> Handle(GetOrderStatusMapQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = OrderStatus.All.Select(s => new { Id = s, Name = OrderStatus.GetDisplayName(s) });

        return Task.FromResult(Result<IEnumerable<object>>.Success(result));
    }
}
