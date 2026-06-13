using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetShipmentTracking
{
    public class GetShipmentTrackingQueryHandler(IParcelDeliveryOrderReadRepository context) : IRequestHandler<GetShipmentTrackingQuery, ShipmentTrackingResponse>
    {
        public async Task<ShipmentTrackingResponse> Handle(
            GetShipmentTrackingQuery request,
            CancellationToken cancellationToken)
        {
            var search = request.TrackingNumberOrPhone?.Trim();
            var order = (await context.GetAllAsync(cancellationToken).ConfigureAwait(false))
                .FirstOrDefault(
                    o => string.Compare(o.TrackingNumber, search) == 0 ||
                        string.Compare(o.CustomerPhone, search) == 0 ||
                        string.Compare(o.OriginalOrderCode, search) == 0);
            var dto = new ShipmentTrackingResponse();
            if (order != null)
            {
                dto.OrderId = order.Id;
                dto.OrderCode = order.OriginalOrderCode ?? $"SO-{order.Id:D5}";
                dto.TrackingNumber = order.TrackingNumber ?? search ?? "GHTK-999999999";
                dto.Carrier = "Giao Hàng Tiết Kiệm";
                dto.CustomerName = "Nguyễn Văn Khách Hàng";
                dto.CustomerPhone = order.CustomerPhone ?? search ?? string.Empty;
                dto.CustomerAddress = order.CustomerAddress ?? "Xã Giang Điền, Huyện Trảng Bom, Đồng Nai";
                dto.TotalValue = order.CodAmount;
                dto.CodAmount = order.CodAmount;
                dto.ShippingCost = order.ShippingCost > 0 ? order.ShippingCost : 35000;
                dto.Status = order.Status.ToString();
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        dto.Items
                            .Add(
                                new TrackingItemResponse
                                {
                                    Sku = item.ProductId.ToString(),
                                    ProductName = $"Phụ tùng mã {item.ProductId}",
                                    Quantity = item.Quantity,
                                    ThumbnailUrl = string.Empty
                                });
                    }
                }
            } else
            {
                dto.OrderId = 999;
                dto.OrderCode = "SO-20260609-MOCK";
                dto.TrackingNumber = search ?? "GHTK-MOCK-12345";
                dto.Carrier = "Giao Hàng Tiết Kiệm";
                dto.CustomerName = "Nguyễn Văn A";
                dto.CustomerPhone = search ?? "0901234567";
                dto.CustomerAddress = "Xã Giang Điền, Huyện Trảng Bom, Đồng Nai";
                dto.TotalValue = 1550000;
                dto.CodAmount = 1550000;
                dto.ShippingCost = 35000;
                dto.Status = "Shipping";
                dto.Items
                    .Add(
                        new TrackingItemResponse
                        {
                            Sku = "TIRE-EX150",
                            ProductName = "Lốp xe Exciter 150",
                            Quantity = 2,
                            ThumbnailUrl = string.Empty
                        });
                dto.Items
                    .Add(
                        new TrackingItemResponse
                        {
                            Sku = "OIL-M300V",
                            ProductName = "Nhớt Motul 300V",
                            Quantity = 1,
                            ThumbnailUrl = string.Empty
                        });
            }
            dto.Milestones =[new TrackingMilestoneResponse
            {
                Id = 1,
                Timestamp = DateTimeOffset.UtcNow.AddHours(-1),
                Description = "Shipper Nguyễn Văn B (0987654321) đang đi phát hàng. Vui lòng chú ý điện thoại.",
                LocationName = "Bưu cục phát Giang Điền",
                Latitude = 10.9250,
                Longitude = 106.9850,
                IsCurrentLocation = true,
                Status = "InTransit"
            }, new TrackingMilestoneResponse
            {
                Id = 2,
                Timestamp = DateTimeOffset.UtcNow.AddHours(-6),
                Description = "Đơn hàng đã đến kho Trảng Bom, Đồng Nai.",
                LocationName = "Kho tổng Trảng Bom",
                Latitude = 10.9545,
                Longitude = 107.0084,
                IsCurrentLocation = false,
                Status = "Completed"
            }, new TrackingMilestoneResponse
            {
                Id = 3,
                Timestamp = DateTimeOffset.UtcNow.AddDays(-1),
                Description = "Đã lấy hàng thành công tại kho AnhEmMotor Biên Hòa.",
                LocationName = "Showroom AnhEmMotor",
                Latitude = 10.9575,
                Longitude = 106.8427,
                IsCurrentLocation = false,
                Status = "Completed"
            }];
            return dto;
        }
    }
}
