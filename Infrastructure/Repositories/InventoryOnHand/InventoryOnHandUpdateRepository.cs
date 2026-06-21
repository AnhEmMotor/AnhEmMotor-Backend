using Application.Interfaces.Repositories.InventoryOnHand;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandUpdateRepository(ApplicationDBContext context) : IInventoryOnHandUpdateRepository
{
    public void Update(InventoryOnHandEntity inventoryOnHand)
    {
        context.InventoryOnHands.Update(inventoryOnHand);
    }

    public async Task RecalculateAsync(
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken)
    {
        var firstReceiptDate = await context.InventoryReceiptInfos
            .Where(x => x.InventoryReceipt != null && x.InventoryReceipt.StatusId == InventoryReceiptStatus.Approve)
            .Where(x => (x.PurchaseRequestItem != null && x.PurchaseRequestItem.ProductVariantId == productVariantId && x.PurchaseRequestItem.ProductVariantColorId == productVariantColorId) ||
                        (x.ParentOutputInfo != null && x.ParentOutputInfo.ProductVariantId == productVariantId && x.ParentOutputInfo.ProductVariantColorId == productVariantColorId))
            .Select(x => (DateTimeOffset?)x.InventoryReceipt!.CreatedAt)
            .MinAsync(cancellationToken)
            .ConfigureAwait(false);

        var firstOutputDate = await context.OutputInfos
            .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
            .Where(x => x.OutputOrder != null && x.OutputOrder.StatusId == OrderStatus.Completed)
            .Select(x => (DateTimeOffset?)x.OutputOrder!.CreatedAt)
            .MinAsync(cancellationToken)
            .ConfigureAwait(false);

        var earliestDate = firstReceiptDate != null && firstOutputDate != null
            ? (firstReceiptDate < firstOutputDate ? firstReceiptDate.Value : firstOutputDate.Value)
            : firstReceiptDate ?? firstOutputDate;

        if (earliestDate == null)
        {
            // No transactions ever. Just make sure current month has 0.
            earliestDate = DateTimeOffset.UtcNow;
        }

        var currentDate = DateTimeOffset.UtcNow;
        var startMonth = earliestDate.Value.Month;
        var startYear = earliestDate.Value.Year;
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;

        int previousStockQty = 0;

        for (int y = startYear; y <= currentYear; y++)
        {
            int mStart = (y == startYear) ? startMonth : 1;
            int mEnd = (y == currentYear) ? currentMonth : 12;

            for (int m = mStart; m <= mEnd; m++)
            {
                var startDate = new DateTimeOffset(y, m, 1, 0, 0, 0, TimeSpan.Zero);
                var endDate = startDate.AddMonths(1);

                var importedQty = await context.InventoryReceiptInfos
                    .Where(x => x.InventoryReceipt != null && x.InventoryReceipt.StatusId == InventoryReceiptStatus.Approve &&
                                x.InventoryReceipt.CreatedAt >= startDate && x.InventoryReceipt.CreatedAt < endDate)
                    .Where(x => (x.PurchaseRequestItem != null && x.PurchaseRequestItem.ProductVariantId == productVariantId && x.PurchaseRequestItem.ProductVariantColorId == productVariantColorId) ||
                                (x.ParentOutputInfo != null && x.ParentOutputInfo.ProductVariantId == productVariantId && x.ParentOutputInfo.ProductVariantColorId == productVariantColorId))
                    .SumAsync(x => x.Count ?? 0, cancellationToken)
                    .ConfigureAwait(false);

                var exportedQty = await context.OutputInfos
                    .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
                    .Where(x => x.OutputOrder != null && x.OutputOrder.StatusId == OrderStatus.Completed &&
                                x.OutputOrder.CreatedAt >= startDate && x.OutputOrder.CreatedAt < endDate)
                    .SumAsync(x => x.Count ?? 0, cancellationToken)
                    .ConfigureAwait(false);

                int orderedQty = 0;
                if (y == currentYear && m == currentMonth)
                {
                    var orderedStatuses = new[]
                    {
                        OrderStatus.Pending, OrderStatus.WaitingDeposit, OrderStatus.DepositPaid,
                        OrderStatus.WaitingInstallment, OrderStatus.InstallmentApproved,
                        OrderStatus.ConfirmedCod, OrderStatus.PaidProcessing
                    };
                    orderedQty = await context.OutputInfos
                        .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
                        .Where(x => x.OutputOrder != null && orderedStatuses.Contains(x.OutputOrder.StatusId))
                        .SumAsync(x => x.Count ?? 0, cancellationToken)
                        .ConfigureAwait(false);
                }

                var inventoryOnHand = await context.InventoryOnHands
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId && x.Month == m && x.Year == y,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (inventoryOnHand == null)
                {
                    inventoryOnHand = new InventoryOnHandEntity
                    {
                        ProductVariantId = productVariantId,
                        ProductVariantColorId = productVariantColorId,
                        Month = m,
                        Year = y
                    };
                    context.InventoryOnHands.Add(inventoryOnHand);
                }

                inventoryOnHand.BeginningQty = previousStockQty;
                inventoryOnHand.ImportedQty = importedQty;
                inventoryOnHand.ExportedQty = exportedQty;
                inventoryOnHand.StockQty = previousStockQty + importedQty - exportedQty;
                inventoryOnHand.OrderedQty = orderedQty;
                inventoryOnHand.DeletedAt = null;

                previousStockQty = inventoryOnHand.StockQty;
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RecalculateAllAsync(CancellationToken cancellationToken)
    {
        var variantColors = await context.ProductVariantColors
            .Select(x => new { x.ProductVariantId, ProductVariantColorId = (int?)x.Id })
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variants = await context.ProductVariants
            .Select(x => new { ProductVariantId = x.Id, ProductVariantColorId = (int?)null })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var allCombinations = variantColors.Concat(variants)
            .GroupBy(x => new { x.ProductVariantId, x.ProductVariantColorId })
            .Select(g => g.Key)
            .ToList();
        foreach (var combo in allCombinations)
        {
            await RecalculateAsync(combo.ProductVariantId, combo.ProductVariantColorId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
