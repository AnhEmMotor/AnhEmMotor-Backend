using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Logistics.Queries.GetActiveShipments
{
    public class GetActiveShipmentsQueryHandler(IShipmentReadRepository db) : IRequestHandler<GetActiveShipmentsQuery, List<ActiveShipmentResponse>>
    {
        public async Task<List<ActiveShipmentResponse>> Handle(
            GetActiveShipmentsQuery request,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var shipments = await db.GetAllAsync(cancellationToken);
            var activeShipments = shipments.Where(
                x => x.Status == ParcelDeliveryStatus.Shipping && !x.DeliveredAt.HasValue)
                .ToList();
            var result = activeShipments.Select(
                x => new ActiveShipmentResponse
                {
                    Id = x.Id,
                    TrackingNumber = x.TrackingNumber ?? string.Empty,
                    CustomerName = x.CustomerName ?? string.Empty,
                    CustomerPhone = x.CustomerPhone ?? string.Empty,
                    CustomerAddress = x.DestinationAddress ?? string.Empty,
                    Carrier = x.Carrier ?? string.Empty,
                    Status = 2,
                    CodAmount = x.CodAmount,
                    ShippingCost = x.ShippingCost,
                    CreatedAt = x.CreatedAt?.DateTime ?? DateTime.MinValue,
                    ExpectedAt = null,
                    DaysInTransit = (int)(now - (x.CreatedAt ?? now)).TotalDays,
                    IsStuck = (now - (x.CreatedAt ?? now)).TotalHours > 48
                })
                .ToList();
            return result;
        }
    }
}
