using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class SalesContractSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.SalesContracts.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;
        var customers = await context.Users
            .Where(u => u.Email == "nam.nguyen@gmail.com" || u.Email == "nguyenvana@gmail.com")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var invoices = await context.Invoices
            .OrderBy(o => o.Id)
            .Take(300)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (customers.Count == 0 || invoices.Count == 0)
            return;
        var random = new Random(77);
        var now = DateTimeOffset.UtcNow;
        var statuses = new[]
        {
            SalesContractStatus.Draft,
            SalesContractStatus.PendingApproval,
            SalesContractStatus.Approved,
            SalesContractStatus.Signed,
            SalesContractStatus.Fulfilled
        };
        var contracts = new List<SalesContract>();
        foreach (var invoice in invoices.Take(40))
        {
            var price = invoice.VehiclePrice;
            var deposit = Math.Round(price * 0.1m / 100000) * 100000;
            var remaining = price - deposit;
            var signedDate = invoice.CreatedAt.HasValue
                ? invoice.CreatedAt.Value.AddDays(random.Next(1, 3))
                : now.AddDays(-random.Next(1, 7));
            var status = statuses[random.Next(statuses.Length)];
            var contract = new SalesContract
            {
                Id = Guid.NewGuid(),
                ContractNumber = $"HDXC-{now.Year}-{contracts.Count + 1:D4}",
                InvoiceId = invoice.Id,
                CustomerId = customers.First().Id, // Fetch from customer table
                ShowroomName = "Anh Em Motor Showroom",
                ShowroomTaxCode = $"0{random.Next(10000, 99999)}{random.Next(10000, 99999)}",
                ShowroomAddress = "123 Nguyễn Trãi, Thanh Xuân, Hà Nội",
                ShowroomRepresentative = "Nguyễn Văn A",
                CustomerFullName = invoice.CustomerName ?? "Khách Hàng",
                CustomerCCCD = invoice.CustomerIdCard ?? $"0{random.Next(100000, 999999)}{random.Next(100000, 999999)}",
                CustomerAddress = invoice.CustomerAddress ?? "Hà Nội",
                CustomerPhone = invoice.CustomerPhone ?? $"+84{random.Next(900000000, 999999999)}",
                VehicleModel = invoice.VehicleModel ?? "Xe máy",
                VehicleVersion = invoice.VehicleVersion ?? "Tiêu chuẩn",
                VehicleColor = invoice.VehicleColor ?? "Đen",
                FrameNumber = invoice.ChassisNo ?? $"RLH{random.Next(100000, 999999)}",
                EngineNumber = invoice.EngineNo ?? $"E{random.Next(10000, 99999)}",
                ActualSalePrice = price,
                DepositAmount = deposit,
                RemainingAmount = remaining,
                FinalPaymentDeadline = signedDate.AddDays(30),
                WarrantyPeriod = "2 năm",
                WarrantyScope = "Bảo hành chính hãng toàn quốc",
                Status = status,
                SignedDate = status == SalesContractStatus.Draft ? null : signedDate,
                ScannedFileUrl =
                    status == SalesContractStatus.Signed || status == SalesContractStatus.Fulfilled
                        ? "/uploads/contracts/sample.pdf"
                        : null,
                Note = status == SalesContractStatus.Fulfilled ? "Đã thanh toán đủ" : null,
                CreatedAt = invoice.CreatedAt ?? now.AddDays(-random.Next(1, 30)),
                UpdatedAt = now.AddDays(-random.Next(0, 7))
            };
            contracts.Add(contract);
        }
        context.SalesContracts.AddRange(contracts);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
