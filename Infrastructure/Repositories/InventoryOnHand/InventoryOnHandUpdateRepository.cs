using Application.Interfaces.Repositories.InventoryOnHand;
using Domain.Entities;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Constants.Booking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandUpdateRepository(ApplicationDBContext context) : IInventoryOnHandUpdateRepository
{
    public void Update(InventoryOnHandEntity inventoryOnHand)
    {
        context.InventoryOnHands.Update(inventoryOnHand);
    }

    public async Task RecalculateAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken)
    {
        // Calculate ImportedQty
        var importedQty = await context.InventoryReceiptInfos
            .Where(x => x.InventoryReceipt != null && x.InventoryReceipt.StatusId == Domain.Constants.InventoryReceiptStatus.Approve)
            .Where(x => 
                (x.PurchaseRequestItem != null && x.PurchaseRequestItem.ProductVariantId == productVariantId && x.PurchaseRequestItem.ProductVariantColorId == productVariantColorId) ||
                (x.ParentOutputInfo != null && x.ParentOutputInfo.ProductVariantId == productVariantId && x.ParentOutputInfo.ProductVariantColorId == productVariantColorId)
            )
            .SumAsync(x => x.Count ?? 0, cancellationToken);

        // Calculate ExportedQty
        var exportedQty = await context.OutputInfos
            .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
            .Where(x => x.OutputOrder != null && x.OutputOrder.StatusId == OrderStatus.Completed)
            .SumAsync(x => x.Count ?? 0, cancellationToken);

        // Calculate OrderedQty
        var orderedStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.WaitingDeposit,
            OrderStatus.DepositPaid,
            OrderStatus.WaitingInstallment,
            OrderStatus.InstallmentApproved,
            OrderStatus.ConfirmedCod,
            OrderStatus.PaidProcessing
        };
        var orderedQty = await context.OutputInfos
            .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
            .Where(x => x.OutputOrder != null && orderedStatuses.Contains(x.OutputOrder.StatusId))
            .SumAsync(x => x.Count ?? 0, cancellationToken);


        var inventoryOnHand = await context.InventoryOnHands
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId, cancellationToken);

        if (inventoryOnHand == null)
        {
            inventoryOnHand = new InventoryOnHandEntity
            {
                ProductVariantId = productVariantId,
                ProductVariantColorId = productVariantColorId
            };
            context.InventoryOnHands.Add(inventoryOnHand);
        }

        inventoryOnHand.ImportedQty = importedQty;
        inventoryOnHand.ExportedQty = exportedQty;
        inventoryOnHand.StockQty = importedQty - exportedQty;
        inventoryOnHand.OrderedQty = orderedQty;

        inventoryOnHand.DeletedAt = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RecalculateAllAsync(CancellationToken cancellationToken)
    {
        var variantColors = await context.ProductVariantColors
            .Select(x => new { x.ProductVariantId, ProductVariantColorId = (int?)x.Id })
            .Distinct()
            .ToListAsync(cancellationToken);

        var variants = await context.ProductVariants
            .Select(x => new { ProductVariantId = x.Id, ProductVariantColorId = (int?)null })
            .ToListAsync(cancellationToken);

        var allCombinations = variantColors.Concat(variants)
            .GroupBy(x => new { x.ProductVariantId, x.ProductVariantColorId })
            .Select(g => g.Key)
            .ToList();

        foreach (var combo in allCombinations)
        {
            await RecalculateAsync(combo.ProductVariantId, combo.ProductVariantColorId, cancellationToken);
        }
    }
}
