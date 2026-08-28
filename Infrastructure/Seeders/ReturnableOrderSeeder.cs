using Domain.Constants.Order;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ReturnableOrderSeeder
{
    private static readonly string[] TransactionIds =
    [
        "SEED-RETURN-ONLINE-001",
        "SEED-RETURN-ONLINE-002",
        "SEED-RETURN-ONLINE-003"
    ];

    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var existingTransactionIds = await context.OutputOrders
            .Where(output => output.TransactionId != null && TransactionIds.Contains(output.TransactionId))
            .Select(output => output.TransactionId!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var missingTransactionIds = TransactionIds.Except(existingTransactionIds).ToArray();
        if (missingTransactionIds.Length == 0)
            return;

        var variants = await context.ProductVariants
            .Include(variant => variant.ProductVariantColors)
            .OrderBy(variant => variant.Id)
            .Take(TransactionIds.Length)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (variants.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        var customers = new[]
        {
            new { Name = "Nguyễn Minh Anh", Phone = "0901000001", Address = "12 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh" },
            new { Name = "Trần Hoàng Nam", Phone = "0901000002", Address = "45 Lê Lợi, Quận 1, TP. Hồ Chí Minh" },
            new { Name = "Lê Thu Trang", Phone = "0901000003", Address = "88 Hai Bà Trưng, Quận 1, TP. Hồ Chí Minh" }
        };

        foreach (var transactionId in missingTransactionIds)
        {
            var seedIndex = Array.IndexOf(TransactionIds, transactionId);
            var variant = variants[seedIndex % variants.Count];
            var color = variant.ProductVariantColors.OrderBy(item => item.Id).FirstOrDefault();
            var customer = customers[seedIndex];
            var quantity = seedIndex + 1;
            var unitPrice = variant.Price ?? 600000m;
            var shippingFee = 30000m + seedIndex * 5000m;
            var completedAt = now.AddDays(-(seedIndex + 3));

            context.OutputOrders.Add(new Output
            {
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                CustomerAddress = customer.Address,
                TransactionId = transactionId,
                StatusId = OrderStatus.Completed,
                PaymentStatus = "Paid",
                PaymentMethod = "Banking",
                PaidAmount = unitPrice * quantity + shippingFee,
                ShippingFee = shippingFee,
                PaidAt = completedAt,
                ProvinceId = 202,
                ProvinceName = "Hồ Chí Minh",
                WardName = "Phường Bến Nghé",
                DepositRatio = 100,
                LastStatusChangedAt = completedAt,
                CreatedAt = completedAt.AddDays(-2),
                UpdatedAt = completedAt,
                Notes = "Dữ liệu seed phục vụ kiểm thử trả hàng",
                OutputInfos =
                [
                    new OutputInfo
                    {
                        ProductVariantId = variant.Id,
                        ProductVariantColorId = color?.Id,
                        Count = quantity,
                        Price = unitPrice,
                        CostPrice = unitPrice * 0.8m,
                        CreatedAt = completedAt.AddDays(-2),
                        UpdatedAt = completedAt
                    }
                ]
            });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
