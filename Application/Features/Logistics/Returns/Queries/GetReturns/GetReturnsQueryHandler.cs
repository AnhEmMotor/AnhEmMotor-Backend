using Application.ApiContracts.Return.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Queries.GetReturns
{
    public class GetReturnsQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository) : IRequestHandler<GetReturnsQuery, List<ReturnOrderResponse>>
    {
        public async Task<List<ReturnOrderResponse>> Handle(
            GetReturnsQuery request,
            CancellationToken cancellationToken)
        {
            var returns = await parcelDeliveryOrderReadRepository.GetReturnedAsync(cancellationToken)
                .ConfigureAwait(false);
            var filtered = request.Status switch
            {
                ReturnOrderStatus.Pending => [.. returns.Where(x => x.InspectedAt == null && string.IsNullOrWhiteSpace(x.ReturnAction))],
                ReturnOrderStatus.Inspecting => [.. returns.Where(x => x.InspectedAt != null && string.IsNullOrWhiteSpace(x.ReturnAction))],
                ReturnOrderStatus.Completed => [.. returns.Where(x => !string.IsNullOrWhiteSpace(x.ReturnAction))],
                _ => returns
            };
            return[.. filtered
            .OrderByDescending(x => x.CreatedAt)
                .Select(
                    x => new ReturnOrderResponse
                    {
                        Id = x.Id,
                        OriginalTrackingNumber = x.TrackingNumber ?? string.Empty,
                        CustomerName = x.CustomerName ?? string.Empty,
                        Carrier = x.Carrier ?? string.Empty,
                        Reason = x.ReturnReason ?? "Khach bom hang",
                        Status = GetReturnStatus(x),
                        CreatedAt = x.CreatedAt
                    })];
        }

        private static ReturnOrderStatus GetReturnStatus(ParcelDeliveryOrder order)
        {
            if (!string.IsNullOrWhiteSpace(order.ReturnAction))
                return ReturnOrderStatus.Completed;
            if (order.InspectedAt != null)
                return ReturnOrderStatus.Inspecting;
            return ReturnOrderStatus.Pending;
        }
    }
}
