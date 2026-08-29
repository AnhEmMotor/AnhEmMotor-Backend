using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class InvoiceSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.Invoices.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var user = await context.Users.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var userId = user?.Id ?? Guid.NewGuid();

        var random = new Random(42);
        var now = DateTime.UtcNow;

        var vehicleList = new[]
        {
            new { Model = "Honda SH 160i ABS", Color = "Đen nhám", Price = 101000000m, RegFee = 5000000m, InsFee = 660000m },
            new { Model = "Honda SH 125i CBS", Color = "Trắng ngọc trai", Price = 75000000m, RegFee = 4000000m, InsFee = 660000m },
            new { Model = "Honda Vision Cao Cấp", Color = "Xanh đen", Price = 33000000m, RegFee = 2000000m, InsFee = 660000m },
            new { Model = "Honda Vision Thể Thao", Color = "Xám xi măng", Price = 37000000m, RegFee = 2200000m, InsFee = 660000m },
            new { Model = "Honda Air Blade 160", Color = "Đỏ đen", Price = 56500000m, RegFee = 3200000m, InsFee = 660000m },
            new { Model = "Honda Air Blade 125", Color = "Xanh xám", Price = 42500000m, RegFee = 2500000m, InsFee = 660000m },
            new { Model = "Yamaha Exciter 155 VVA", Color = "Xanh GP", Price = 52000000m, RegFee = 3000000m, InsFee = 660000m },
            new { Model = "Yamaha Grande Hybrid", Color = "Trắng ánh kim", Price = 49000000m, RegFee = 2800000m, InsFee = 660000m },
            new { Model = "Honda Winner X ABS", Color = "Đỏ bạc đen", Price = 46000000m, RegFee = 2700000m, InsFee = 660000m },
            new { Model = "Vespa Sprint S 150", Color = "Vàng nhám", Price = 98000000m, RegFee = 5500000m, InsFee = 660000m },
            new { Model = "Honda Vario 160", Color = "Đen mờ", Price = 51500000m, RegFee = 3000000m, InsFee = 660000m },
            new { Model = "Yamaha NVX 155 VVA", Color = "Đen tem vàng", Price = 54500000m, RegFee = 3100000m, InsFee = 660000m }
        };

        var customers = new[]
        {
            new { Name = "Nguyễn Văn Hùng", Phone = "0988123456", CCCD = "001201004567", Address = "12 P. Cầu Giấy, Quan Hoa, Cầu Giấy, Hà Nội" },
            new { Name = "Trần Thị Mai Anh", Phone = "0977234567", CCCD = "001202008912", Address = "45 Nguyễn Trãi, Thanh Xuân Bắc, Thanh Xuân, Hà Nội" },
            new { Name = "Lê Hoàng Long", Phone = "0912345678", CCCD = "001200001234", Address = "88 Hoàng Quốc Việt, Nghĩa Đô, Cầu Giấy, Hà Nội" },
            new { Name = "Phạm Thị Thu Trang", Phone = "0934567890", CCCD = "001203005678", Address = "102 Xã Đàn, Nam Đồng, Đống Đa, Hà Nội" },
            new { Name = "Đỗ Minh Quân", Phone = "0901234501", CCCD = "001201009876", Address = "234 Láng Hạ, Đống Đa, Hà Nội" },
            new { Name = "Vũ Thị Giang", Phone = "0956789006", CCCD = "001202004321", Address = "67 Giải Phóng, Đồng Tâm, Hai Bà Trưng, Hà Nội" },
            new { Name = "Đỗ Văn Huy", Phone = "0967890107", CCCD = "001200007654", Address = "89 Minh Khai, Vĩnh Tuy, Hai Bà Trưng, Hà Nội" },
            new { Name = "Lâm Thị Kim", Phone = "0978901208", CCCD = "001203002345", Address = "156 Trường Chinh, Khương Mai, Thanh Xuân, Hà Nội" },
            new { Name = "Bùi Văn Long", Phone = "0989012309", CCCD = "001201006789", Address = "312 Tôn Đức Thắng, Hàng Bột, Đống Đa, Hà Nội" },
            new { Name = "Hoàng Tuấn Anh", Phone = "0945678901", CCCD = "001200008765", Address = "78 Lê Đức Thọ, Mỹ Đình, Nam Từ Liêm, Hà Nội" },
            new { Name = "Ngô Bảo Ngọc", Phone = "0923456789", CCCD = "001202001122", Address = "90 Phố Huế, Hàng Bài, Hoàn Kiếm, Hà Nội" },
            new { Name = "Đinh Quốc Cường", Phone = "0961234567", CCCD = "001201003344", Address = "14 Võ Chí Công, Xuân La, Tây Hồ, Hà Nội" }
        };

        var salesPersons = new[] { "Nguyễn Thị Thảo", "Trần Đình Trọng", "Lê Văn Hậu", "Phạm Hải Đăng" };
        var paymentMethods = new[] { "transfer", "cash", "installment" };
        var statuses = new[] { "pending", "processing", "completed", "cancelled" };

        var invoices = new List<Invoice>();
        int count = 1;

        // Ensure at least 6 pending, 6 processing, 8 completed, 2 cancelled
        var preAllocatedStatuses = new List<string>
        {
            "pending", "pending", "pending", "pending", "pending", "pending",
            "processing", "processing", "processing", "processing", "processing", "processing",
            "completed", "completed", "completed", "completed", "completed", "completed", "completed", "completed",
            "cancelled", "cancelled"
        };

        foreach (var status in preAllocatedStatuses)
        {
            var cust = customers[random.Next(customers.Length)];
            var veh = vehicleList[random.Next(vehicleList.Length)];
            var sp = salesPersons[random.Next(salesPersons.Length)];
            var pm = paymentMethods[random.Next(paymentMethods.Length)];
            var daysAgo = random.Next(0, 20);
            var issueDate = now.AddDays(-daysAgo).AddHours(-random.Next(1, 12));

            var totalAmount = veh.Price + veh.RegFee + veh.InsFee;
            var invoiceNumber = $"INV-{issueDate:yyyyMMdd}-{count:D4}";

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                IssueDate = issueDate,
                TotalAmount = totalAmount,
                Type = "vehicle_sales",
                UserId = userId,
                CustomerName = cust.Name,
                CustomerIdCard = cust.CCCD,
                CustomerPhone = cust.Phone,
                CustomerAddress = cust.Address,
                VehicleModel = veh.Model,
                VehicleColor = veh.Color,
                ChassisNo = $"RLH{random.Next(10000000, 99999999)}",
                EngineNo = $"E{random.Next(1000000, 9999999)}",
                VehiclePrice = veh.Price,
                RegistrationFee = veh.RegFee,
                InsuranceFee = veh.InsFee,
                PaymentMethod = pm,
                BankName = pm == "installment" ? "Vietcombank Leasing" : (pm == "transfer" ? "MBBank" : null),
                Status = status,
                SalesPerson = sp,
                DeliveryDate = issueDate.AddDays(random.Next(1, 5)),
                ProcessedBy = status == "completed" ? "Admin Tổng" : (status == "processing" ? "NV Điều phối" : null),
                ProcessedAt = status == "completed" ? issueDate.AddHours(2) : null,
                CreatedAt = issueDate,
                UpdatedAt = issueDate
            };

            invoices.Add(invoice);
            count++;
        }

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
