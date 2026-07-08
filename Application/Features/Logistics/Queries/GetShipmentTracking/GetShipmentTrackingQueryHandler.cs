using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetShipmentTracking
{
    public class GetShipmentTrackingQueryHandler(IShipmentReadRepository context) : IRequestHandler<GetShipmentTrackingQuery, ShipmentTrackingResponse>
    {
        public async Task<ShipmentTrackingResponse> Handle(
            GetShipmentTrackingQuery request,
            CancellationToken cancellationToken)
        {
            var search = request.TrackingNumberOrPhone?.Trim();
            var shipments = await context.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var order = shipments.FirstOrDefault(
                    o => string.Compare(o.TrackingNumber, search) == 0 ||
                        string.Compare(o.CustomerPhone, search) == 0 ||
                        o.OutputId.ToString() == search);
            var dto = new ShipmentTrackingResponse();
            if (order != null)
            {
                dto.OrderId = order.Id;
                dto.OrderCode = order.OutputId?.ToString() ?? $"SO-{order.Id:D5}";
                dto.TrackingNumber = order.TrackingNumber;
                dto.Carrier = order.Carrier;
                dto.CustomerName = order.CustomerName;
                dto.CustomerPhone = order.CustomerPhone;
                dto.CustomerAddress = order.DestinationAddress;
                dto.TotalValue = order.CodAmount;
                dto.CodAmount = order.CodAmount;
                dto.ShippingCost = order.ShippingCost;
                dto.Status = order.DeliveredAt.HasValue ? "Delivered" : "InTransit";
                dto.OriginLatitude = order.OriginLatitude;
                dto.OriginLongitude = order.OriginLongitude;
                dto.DestinationLatitude = order.DestinationLatitude;
                dto.DestinationLongitude = order.DestinationLongitude;
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        dto.Items
                            .Add(
                                new TrackingItemResponse
                                {
                                    ProductName = GenerateProductName(item),
                                    Quantity = item.Quantity,
                                    ThumbnailUrl = item.ProductVariantColor?.CoverImageUrl ?? item.ProductVariant?.CoverImageUrl ?? string.Empty
                                });
                    }
                }
            } 
            
            dto.Milestones = [];
            return dto;
        }

        private static string GenerateProductName(Domain.Entities.Logistics.ShipmentItem item)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.ProductVariant?.Product?.Name))
                parts.Add(item.ProductVariant.Product.Name);
            if (!string.IsNullOrWhiteSpace(item.ProductVariant?.VariantName))
                parts.Add(item.ProductVariant.VariantName);
            if (!string.IsNullOrWhiteSpace(item.ProductVariantColor?.ColorName))
                parts.Add(item.ProductVariantColor.ColorName);
            return string.Join(" - ", parts);
        }
    }
}
