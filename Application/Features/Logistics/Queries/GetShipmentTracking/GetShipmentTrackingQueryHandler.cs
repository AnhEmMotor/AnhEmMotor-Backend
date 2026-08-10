using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using Domain.Enums;
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
                if (dto.OriginLatitude == 0 && dto.OriginLongitude == 0)
                {
                    dto.OriginLatitude = 10.9576;
                    dto.OriginLongitude = 106.8427;
                }
                if (dto.DestinationLatitude == 0 && dto.DestinationLongitude == 0)
                {
                    dto.DestinationLatitude = 10.7626;
                    dto.DestinationLongitude = 106.6602;
                }
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
                                    ThumbnailUrl =
                                        item.ProductVariantColor?.CoverImageUrl ??
                                                item.ProductVariant?.CoverImageUrl ??
                                                string.Empty
                                });
                    }
                }
            }
            if (order != null)
            {
                var milestones = new List<TrackingMilestoneResponse>();
                var baseDate = order.CreatedAt?.UtcDateTime ?? DateTime.UtcNow.AddDays(-2);
                milestones.Add(
                    new TrackingMilestoneResponse
                    {
                        Timestamp = baseDate,
                        Location = "Showroom AnhEmMotor Biên Hòa",
                        Status = "Đã lấy hàng",
                        IsCurrent = false,
                        Latitude = dto.OriginLatitude,
                        Longitude = dto.OriginLongitude
                    });
                if (order.Status == ParcelDeliveryStatus.Shipping || order.Status == ParcelDeliveryStatus.Completed)
                {
                    milestones.Add(
                        new TrackingMilestoneResponse
                        {
                            Timestamp = baseDate.AddHours(4),
                            Location = "Bưu cục trung chuyển Đồng Nai",
                            Status = "Đã đến bưu cục trung chuyển",
                            IsCurrent = order.Status == ParcelDeliveryStatus.Shipping,
                            Latitude = (dto.OriginLatitude + dto.DestinationLatitude) / 2,
                            Longitude = (dto.OriginLongitude + dto.DestinationLongitude) / 2
                        });
                }
                if (order.Status == ParcelDeliveryStatus.Completed && order.DeliveredAt.HasValue)
                {
                    milestones.Add(
                        new TrackingMilestoneResponse
                        {
                            Timestamp = order.DeliveredAt.Value.UtcDateTime,
                            Location = order.DestinationAddress ?? "Địa chỉ người nhận",
                            Status = "Giao hàng thành công",
                            IsCurrent = true,
                            Latitude = dto.DestinationLatitude,
                            Longitude = dto.DestinationLongitude
                        });
                }
                dto.Milestones = milestones;
            }
            return dto;
        }

        private static string GenerateProductName(ShipmentItem item)
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
