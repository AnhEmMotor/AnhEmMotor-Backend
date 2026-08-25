using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders
{
    public static class ReturnRequestSeeder
    {
        public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
        {
            if (await context.ReturnRequests.CountAsync(cancellationToken) > 5)
            {
                return;
            }

            var outputs = await context.OutputOrders
                .Include(o => o.OutputInfos)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .Where(o => o.OutputInfos.Any())
                .Take(20)
                .ToListAsync(cancellationToken);

            if (outputs.Count == 0)
            {
                return;
            }

            var random = new Random(42);
            var statuses = new[] { "pending", "inspecting", "completed", "rejected" };
            var types = new[] { "return", "cancel" };
            var reasons = new[] {
                "Sản phẩm không đúng mô tả",
                "Sản phẩm bị lỗi kỹ thuật",
                "Sản phẩm bị trầy xước",
                "Giao sai màu sắc",
                "Khách hàng đổi ý",
                "Giao hàng quá chậm"
            };

            var requests = new List<ReturnRequest>();

            for (int i = 0; i < 15; i++)
            {
                var output = outputs[random.Next(outputs.Count)];
                var status = statuses[random.Next(statuses.Length)];
                var type = types[random.Next(types.Length)];
                var reason = reasons[random.Next(reasons.Length)];

                var returnRequest = new ReturnRequest
                {
                    OrderId = output.Id,
                    OrderCode = $"ORD-2026-{(output.Id):D4}",
                    CustomerName = output.CustomerName ?? $"Khách hàng {i}",
                    CustomerPhone = output.CustomerPhone ?? "0901234567",
                    Carrier = "GHTK",
                    Type = type,
                    Status = status,
                    Reason = reason,
                    Note = "Seeded data",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 30))
                };

                if (status == "completed" || status == "inspecting")
                {
                    returnRequest.InspectedAt = returnRequest.CreatedAt.GetValueOrDefault().AddDays(1);
                    if (status == "completed")
                    {
                        returnRequest.ReturnAction = "refund";
                    }
                }
                else if (status == "rejected")
                {
                    returnRequest.RejectionReason = "Sản phẩm vẫn hoạt động bình thường, không tìm thấy lỗi.";
                }

                foreach (var info in output.OutputInfos.Take(2))
                {
                    if (info.ProductVariant?.Product != null)
                    {
                        var item = new ReturnRequestItem
                        {
                            ProductId = info.ProductVariant.Product.Id,
                            ProductName = info.ProductVariant.Product.Name ?? "Sản phẩm",
                            Sku = info.ProductVariant.Sku ?? "",
                            Quantity = info.Count ?? 1,
                            ReturnQuantity = random.Next(1, (info.Count ?? 1) + 1),
                            UnitPrice = info.Price ?? 0m
                        };
                        returnRequest.Items.Add(item);
                    }
                }

                if (returnRequest.Items.Any())
                {
                    requests.Add(returnRequest);
                }
            }

            context.ReturnRequests.AddRange(requests);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
