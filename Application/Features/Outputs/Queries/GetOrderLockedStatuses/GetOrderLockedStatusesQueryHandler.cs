using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderLockedStatuses;

public sealed class GetOrderLockedStatusesQueryHandler : IRequestHandler<GetOrderLockedStatusesQuery, Result<OrderLockStatusResponse>>
{
    public Task<Result<OrderLockStatusResponse>> Handle(
        GetOrderLockedStatusesQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new OrderLockStatusResponse
        {
            BuyerAndProducts = OrderLockStatus.BuyerAndProductsLockedStatuses,
            DeliveryInfo = OrderLockStatus.DeliveryInfoLockedStatuses,
            Notes = OrderLockStatus.NotesLockedStatuses
        };
        return Task.FromResult(Result<OrderLockStatusResponse>.Success(result));
    }
}
