using Domain.Entities.Logistics;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders;

public static class LogisticsDataSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.ParcelDeliveryOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var parcels = new List<ParcelDeliveryOrder>
        {
            // PENDING/PACKING orders (Fulfillment)
            new()
            {
                CustomerName = "Nguyễn Văn A",
                CustomerPhone = "0987654321",
                CustomerAddress = "123 Đường Ba Tháng Hai, Quận 10, TP. Hồ Chí Minh",
                Carrier = "GHTK",
                TrackingNumber = string.Empty,
                OriginalOrderCode = "ORD-2026-001",
                Status = ParcelDeliveryStatus.Pending,
                CodAmount = 450000m,
                ShippingCost = 35000m,
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 1, ProductName = "Má phanh trước Honda Vision", Sku = "HD-MP-VIS-01", ShelfLocation = "A-12", Quantity = 2, IsPicked = false, IsRestricted = false },
                    new() { ProductId = 2, ProductName = "Dầu nhớt Castrol Power1 1L", Sku = "CAS-POWER1-1L", ShelfLocation = "B-03", Quantity = 1, IsPicked = false, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Trần Thị B",
                CustomerPhone = "0909123456",
                CustomerAddress = "456 Lê Duẩn, Hải Châu, Đà Nẵng",
                Carrier = "GHN",
                TrackingNumber = string.Empty,
                OriginalOrderCode = "ORD-2026-002",
                Status = ParcelDeliveryStatus.Packing,
                CodAmount = 1200000m,
                ShippingCost = 45000m,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 3, ProductName = "Nhông xích xe máy Exciter 150 D.I.D", Sku = "DID-NX-EX150", ShelfLocation = "C-15", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Lê Hoàng C",
                CustomerPhone = "0918777888",
                CustomerAddress = "789 Nguyễn Chí Thanh, Láng Thượng, Đống Đa, Hà Nội",
                Carrier = "ViettelPost",
                TrackingNumber = string.Empty,
                OriginalOrderCode = "ORD-2026-003",
                Status = ParcelDeliveryStatus.Pending,
                CodAmount = 850000m,
                ShippingCost = 40000m,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 4, ProductName = "Bóng đèn pha LED Philips H4", Sku = "PHI-LED-H4", ShelfLocation = "D-20", Quantity = 2, IsPicked = false, IsRestricted = false }
                }
            },

            // SHIPPING orders (Tracking Map)
            new()
            {
                CustomerName = "Phạm Minh D",
                CustomerPhone = "0932888999",
                CustomerAddress = "12 Nguyễn Văn Linh, Bình Hưng, Bình Chánh, TP. Hồ Chí Minh",
                Carrier = "GHTK",
                TrackingNumber = "GHTK882711029",
                OriginalOrderCode = "ORD-2026-010",
                Status = ParcelDeliveryStatus.Shipping,
                CodAmount = 1500000m,
                ShippingCost = 30000m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpectedAt = DateTime.UtcNow.AddDays(1),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 5, ProductName = "Lốp xe không săm Michelin City Grip 2", Sku = "MIC-CG2-100-90", ShelfLocation = "E-05", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Đỗ Hoàng E",
                CustomerPhone = "0977666555",
                CustomerAddress = "345 Trần Hưng Đạo, Ninh Kiều, Cần Thơ",
                Carrier = "GHN",
                TrackingNumber = "GHN992811902",
                OriginalOrderCode = "ORD-2026-011",
                Status = ParcelDeliveryStatus.Shipping,
                CodAmount = 600000m,
                ShippingCost = 35000m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpectedAt = DateTime.UtcNow.AddHours(4),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 6, ProductName = "Kính chiếu hậu xe máy kiểng Rizoma", Sku = "RIZ-CH-K1", ShelfLocation = "F-02", Quantity = 2, IsPicked = true, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Hoàng Thị F",
                CustomerPhone = "0944111222",
                CustomerAddress = "56 Quang Trung, Hồng Bàng, Hải Phòng",
                Carrier = "ViettelPost",
                TrackingNumber = "VTP778899120",
                OriginalOrderCode = "ORD-2026-012",
                Status = ParcelDeliveryStatus.Shipping,
                CodAmount = 0m, // Paid
                ShippingCost = 50000m,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ExpectedAt = DateTime.UtcNow.AddDays(-1), // Late
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 7, ProductName = "Ắc quy xe máy Globe 12V 5Ah", Sku = "GLO-AQ-5AH", ShelfLocation = "G-08", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            },

            // COMPLETED orders
            new()
            {
                CustomerName = "Bùi Văn G",
                CustomerPhone = "0966444888",
                CustomerAddress = "88 Hoàng Hoa Thám, Ba Đình, Hà Nội",
                Carrier = "GHTK",
                TrackingNumber = "GHTK112233445",
                OriginalOrderCode = "ORD-2026-020",
                Status = ParcelDeliveryStatus.Completed,
                CodAmount = 300000m,
                ShippingCost = 30000m,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DeliveredAt = DateTime.UtcNow.AddDays(-3),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 8, ProductName = "Dây curoa Bando Honda SH150", Sku = "BAN-CR-SH150", ShelfLocation = "H-01", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Vũ Thị H",
                CustomerPhone = "0903999888",
                CustomerAddress = "202 Lê Lợi, Vũng Tàu, Bà Rịa - Vũng Tàu",
                Carrier = "GHN",
                TrackingNumber = "GHN556677889",
                OriginalOrderCode = "ORD-2026-021",
                Status = ParcelDeliveryStatus.Completed,
                CodAmount = 2500000m,
                ShippingCost = 40000m,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                DeliveredAt = DateTime.UtcNow.AddDays(-4),
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 9, ProductName = "Phuộc sau xe máy Ohlins bình dầu", Sku = "OHL-PH-BD01", ShelfLocation = "I-10", Quantity = 2, IsPicked = true, IsRestricted = false }
                }
            },

            // RETURNED orders
            new()
            {
                CustomerName = "Đặng Văn I",
                CustomerPhone = "0981222333",
                CustomerAddress = "45 Nguyễn Trãi, Thanh Xuân, Hà Nội",
                Carrier = "GHN",
                TrackingNumber = "GHN776655443",
                OriginalOrderCode = "ORD-2026-030",
                Status = ParcelDeliveryStatus.Returned,
                CodAmount = 500000m,
                ShippingCost = 30000m,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                ReturnReason = "Không nghe máy / Khách bùng hàng",
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 10, ProductName = "Lọc gió xe máy K&N chính hãng", Sku = "KN-LG-01", ShelfLocation = "J-04", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            },
            new()
            {
                CustomerName = "Mai Thị J",
                CustomerPhone = "0934111333",
                CustomerAddress = "123 Cách Mạng Tháng Tám, Quận 3, TP. Hồ Chí Minh",
                Carrier = "GHTK",
                TrackingNumber = "GHTK998877665",
                OriginalOrderCode = "ORD-2026-031",
                Status = ParcelDeliveryStatus.Returned,
                CodAmount = 750000m,
                ShippingCost = 35000m,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                ReturnReason = "Sản phẩm trầy xước trong quá trình ship",
                Items = new List<ParcelDeliveryOrderItem>
                {
                    new() { ProductId = 11, ProductName = "Ốp pô carbon xe máy Click/Vario", Sku = "CAR-OP-CL01", ShelfLocation = "K-02", Quantity = 1, IsPicked = true, IsRestricted = false }
                }
            }
        };

        await context.ParcelDeliveryOrders.AddRangeAsync(parcels, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
