using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetFulfillmentOrders
{
    public class GetFulfillmentOrdersQueryHandler(IShipmentReadRepository shipmentReadRepository) : IRequestHandler<GetFulfillmentOrdersQuery, List<FulfillmentOrderResponse>>
    {
        public async Task<List<FulfillmentOrderResponse>> Handle(
            GetFulfillmentOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var shipments = await shipmentReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var query = shipments.AsEnumerable();
            if (request.Status.HasValue)
            {
                if (request.Status.Value == Domain.Enums.ParcelDeliveryStatus.Completed)
                {
                    query = query.Where(x => x.Status == Domain.Enums.ParcelDeliveryStatus.Completed);
                }
                else if (request.Status.Value == Domain.Enums.ParcelDeliveryStatus.Shipping)
                {
                    query = query.Where(x => x.Status == Domain.Enums.ParcelDeliveryStatus.Shipping);
                }
                else if (request.Status.Value == Domain.Enums.ParcelDeliveryStatus.Returned)
                {
                    query = query.Where(x => x.Status == Domain.Enums.ParcelDeliveryStatus.Returned);
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Carrier))
            {
                query = query.Where(x => string.Equals(x.Carrier, request.Carrier, StringComparison.OrdinalIgnoreCase));
            }
            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
            }
            return query
                .OrderByDescending(x => x.CreatedAt)
                .Select(
                    x => new FulfillmentOrderResponse
                    {
                        Id = x.Id,
                        TrackingNumber = x.TrackingNumber ?? string.Empty,
                        OriginalOrderCode = x.OutputId?.ToString() ?? string.Empty,
                        CustomerName = x.CustomerName ?? string.Empty,
                        CustomerPhone = x.CustomerPhone ?? string.Empty,
                        CustomerAddress = x.DestinationAddress ?? string.Empty,
                        Carrier = x.Carrier ?? string.Empty,
                        Status = x.Status == Domain.Enums.ParcelDeliveryStatus.Shipping && x.DeliveredAt.HasValue 
                            ? Domain.Enums.ParcelDeliveryStatus.Completed 
                            : x.Status,
                        CodAmount = x.CodAmount,
                        ShippingCost = x.ShippingCost,
                        CreatedAt = x.CreatedAt?.DateTime ?? DateTime.MinValue,
                        ExpectedAt = null,
                        DeliveredAt = x.DeliveredAt?.DateTime
                    })
                .ToList();
        }
    }
}
