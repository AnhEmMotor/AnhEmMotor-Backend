using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders;

public static class WorkshopDataSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, IConfiguration configuration, CancellationToken cancellationToken)
    {
        await SeedMaintenanceHistoryAsync(context, cancellationToken);
        await SeedWorkshopPaymentsAsync(context, cancellationToken);
    }

    private static async Task SeedMaintenanceHistoryAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.MaintenanceHistory.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var now = DateTimeOffset.UtcNow;
        var histories = new List<MaintenanceHistory>
        {
            new()
            {
                VehicleId = 1,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-001",
                MaintenanceDate = now.AddMonths(-2),
                Description = "Bao duong dinh ky 5000km: thay nh�t dong co, kiem tra loc gio, siet oc toan xe",
                Mileage = 5000,
                PartsCost = 150000,
                LaborCost = 100000,
                TotalCost = 250000,
                NextMaintenanceDate = now.AddMonths(2),
                NextMaintenanceOdo = 10000,
                PartsJson = "[{\"name\":\"Nh�t t?ng h?p Motul 300V 10W-40\",\"qty\":1,\"price\":150000}]",
            },
            new()
            {
                VehicleId = 1,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-002",
                MaintenanceDate = now.AddMonths(-1),
                Description = "Bao duong 10000km: thay l?c nh?t, l?c gi?, ki?m tra phanh, bo c�n",
                Mileage = 10000,
                PartsCost = 200000,
                LaborCost = 150000,
                TotalCost = 350000,
                NextMaintenanceDate = now.AddMonths(3),
                NextMaintenanceOdo = 15000,
                PartsJson = "[{\"name\":\"L?c gi? d?ng co\",\"qty\":1,\"price\":80000},{\"name\":\"L?c d?u nh?t\",\"qty\":1,\"price\":120000}]",
            },
            new()
            {
                VehicleId = 2,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-003",
                MaintenanceDate = now.AddMonths(-3),
                Description = "Thay nh?t may Castrol Power1 4T 10W-40, kiem tra ap suat lop",
                Mileage = 2000,
                PartsCost = 120000,
                LaborCost = 50000,
                TotalCost = 170000,
                NextMaintenanceDate = now.AddMonths(1),
                NextMaintenanceOdo = 6000,
                PartsJson = "[{\"name\":\"Nh�t Castrol Power1 4T 1.2L\",\"qty\":1,\"price\":120000}]",
            },
            new()
            {
                VehicleId = 3,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-004",
                MaintenanceDate = now.AddMonths(-6),
                Description = "Bao duong toan dien 10000km: thay nh?t, d?y cu?a, bugi, kiem tra h?p s?, ve sinh phun x?ng",
                Mileage = 10000,
                PartsCost = 450000,
                LaborCost = 200000,
                TotalCost = 650000,
                NextMaintenanceDate = now.AddMonths(6),
                NextMaintenanceOdo = 20000,
                PartsJson = "[{\"name\":\"D�y cu?a truyen d?ng\",\"qty\":1,\"price\":180000},{\"name\":\"Bugi Iridium\",\"qty\":1,\"price\":270000}]",
            },
            new()
            {
                VehicleId = 4,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-005",
                MaintenanceDate = now.AddMonths(-2),
                Description = "Thay nh?t xe s?, ve sinh bu?ng d?t, di?u ch?nh xích",
                Mileage = 3000,
                PartsCost = 95000,
                LaborCost = 80000,
                TotalCost = 175000,
                NextMaintenanceDate = now.AddMonths(2),
                NextMaintenanceOdo = 8000,
                PartsJson = "[{\"name\":\"Nh�t xe s? Yamalube 4FS\",\"qty\":1,\"price\":95000}]",
            },
            new()
            {
                VehicleId = 5,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-006",
                MaintenanceDate = now.AddDays(-7),
                Description = "Ki?m tra h? th?ng phanh ABS, c?m bi?n t?c d? b�nh xe, c?p nh?t firmware ECU",
                Mileage = 1500,
                PartsCost = 0,
                LaborCost = 150000,
                TotalCost = 150000,
                NextMaintenanceDate = now.AddMonths(6),
                NextMaintenanceOdo = 10000,
                PartsJson = null,
            },
            new()
            {
                VehicleId = 5,
                TechnicianId = null,
                MaintenanceNumber = "MNT-2026-007",
                MaintenanceDate = now.AddMonths(-1),
                Description = "Ve sinh n?i xe tay ga, thay d?y cu?a, ki?m tra bi tang",
                Mileage = 4500,
                PartsCost = 60000,
                LaborCost = 120000,
                TotalCost = 180000,
                NextMaintenanceDate = now.AddMonths(3),
                NextMaintenanceOdo = 9000,
                PartsJson = "[{\"name\":\"D�y cu?a da tay ga\",\"qty\":1,\"price\":60000}]",
            },
        };

        foreach (var h in histories)
        {
            h.CreatedAt = h.MaintenanceDate;
            h.UpdatedAt = h.MaintenanceDate;
        }

        await context.MaintenanceHistory.AddRangeAsync(histories, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedWorkshopPaymentsAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.WorkshopPayments.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var now = DateTimeOffset.UtcNow;

        // List RepairOrder/Service Booking IDs with real data
        var paymentSources = new List<(string SourceType, int SourceId, string CustomerName, string CustomerPhone, string VehicleInfo, string Service, decimal SubTotal, decimal Discount, decimal Total, string Method, string Status, DateTimeOffset Created)>
        {
            ("RepairOrder", 1, "L� Minh Hi?u", "0901234567", "Vision 2023 - 29A-123.45",
             "Bao duong dinh ky 15000km, thay dau dong co", 350000, 0, 350000, "Cash", "Paid", now.AddDays(-5)),
            ("RepairOrder", 2, "Tr?n Th? Mai", "0912345678", "Winner 2024 - 30B-987.65",
             "Xe b? k�u d�n �o, ki?m tra va si?t �c", 50000, 0, 50000, null, "Unpaid", now.AddHours(-6)),
            ("RepairOrder", 3, "Nguy?n V?n Nam", "0987654321", "Exciter 155 - 31C-111.22",
             "Thay th? nh�ng s�n d?a, thay m? phanh", 1050000, 0, 1050000, null, "Unpaid", now.AddDays(-1)),
            ("RepairOrder", 5, "V? Th? H?ng", "0966778899", "Air Blade 125 - 30E-456.78",
             "Thay nh?t may, nh?t h?p s?", 300000, 0, 300000, "BankTransfer", "Paid", now.AddDays(-3)),
            ("RepairOrder", 8, "Ph?m Minh Tu?n", "0933445566", "Winner X 2024 - 31F-789.12",
             "L�m l?i phu?c tr??c, thay ch�n c?", 700000, 50000, 650000, "Cash", "Paid", now.AddDays(-2)),
            ("RepairOrder", 9, "Ph?m Minh Tu?n", "0933445566", "Winner X 2024 - 31F-789.12",
             "Thay l?p sau, v� l?p tr??c", 600000, 0, 600000, "BankTransfer", "Paid", now.AddDays(-1)),
            ("Maintenance", 1010, "Nguy?n V?n Nam", "0987123456", "Exciter 155 - 31C-111.22",
             "Bao duong dinh ky 5000km", 250000, 0, 250000, "Cash", "Paid", now.AddMonths(-2)),
            ("Maintenance", 1011, "Nguy?n V?n Nam", "0987123456", "Exciter 155 - 31C-111.22",
             "Bao duong 10000km, thay l?c gi?", 350000, 20000, 330000, "BankTransfer", "Paid", now.AddMonths(-1)),
            ("Maintenance", 1012, "Tr?n Th? Mai", "0912345678", "Winner 2024 - 30B-987.65",
             "Thay nh?t may", 170000, 0, 170000, "Cash", "Paid", now.AddMonths(-3)),
        };

        var payments = paymentSources.Select((p, idx) =>
        {
            var wp = new WorkshopPayment
            {
                PaymentNumber = $"WP-2026-{idx + 1:D3}",
                SourceType = p.SourceType,
                SourceId = p.SourceId,
                CustomerName = p.CustomerName,
                CustomerPhone = p.CustomerPhone,
                VehicleInfo = p.VehicleInfo,
                ServiceDescription = p.Service,
                SubTotal = p.SubTotal,
                DiscountAmount = p.Discount,
                TotalAmount = p.Total,
                PaymentMethod = p.Method,
                PaymentStatus = p.Status,
                ReceivedById = null,
                PaidAt = p.Status == "Paid" ? p.Created.AddHours(-2) : null,
                Notes = "",
                InvoicePrintedAt = p.Status == "Paid" ? p.Created.AddHours(-2) : null,
                CreatedAt = p.Created,
                UpdatedAt = p.Created,
            };
            return wp;
        }).ToList();

        await context.WorkshopPayments.AddRangeAsync(payments, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
