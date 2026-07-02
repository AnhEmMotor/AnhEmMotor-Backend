using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Logistics.Queries.GetFulfillmentOrders
{
    public class GetFulfillmentOrdersQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository) 
        : IRequestHandler<GetFulfillmentOrdersQuery, List<FulfillmentOrderResponse>>
    {
        public async Task<List<FulfillmentOrderResponse>> Handle(
            GetFulfillmentOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var parcels = await parcelDeliveryOrderReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            var query = parcels.AsEnumerable();

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
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
                .Select(x => new FulfillmentOrderResponse
                {
                    Id = x.Id,
                    TrackingNumber = x.TrackingNumber ?? string.Empty,
                    OriginalOrderCode = x.OriginalOrderCode ?? string.Empty,
                    CustomerName = x.CustomerName ?? string.Empty,
                    CustomerPhone = x.CustomerPhone ?? string.Empty,
                    CustomerAddress = x.CustomerAddress ?? string.Empty,
                    Carrier = x.Carrier ?? string.Empty,
                    Status = x.Status,
                    CodAmount = x.CodAmount,
                    ShippingCost = x.ShippingCost,
                    CreatedAt = x.CreatedAt,
                    ExpectedAt = x.ExpectedAt,
                    DeliveredAt = x.DeliveredAt
                })
                .ToList();
        }
    }
}
