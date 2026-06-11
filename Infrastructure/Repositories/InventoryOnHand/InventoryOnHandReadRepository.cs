using Application.Interfaces.Repositories.InventoryOnHand;
using Application.ApiContracts.InventoryReport.Responses;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandReadRepository(ApplicationDBContext context) : IInventoryOnHandReadRepository
{
    public Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken)
    {
        return context.InventoryOnHands
            .FirstOrDefaultAsync(
                x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId,
                cancellationToken);
    }

    public Task<List<InventoryReportSummaryRowResponse>> GetInventoryReportSummaryRowsAsync(
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var query = context.InventoryOnHands
            .AsNoTracking()
            .Where(x => x.ProductVariant != null && x.ProductVariant.Product != null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(
                x => (x.ProductVariant!.Product!.Name != null &&
                        x.ProductVariant.Product.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.ProductVariant.VariantName != null &&
                        x.ProductVariant.VariantName
                            .ToLower()
                            .Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.ProductVariantColor != null &&
                        x.ProductVariantColor.ColorName != null &&
                        x.ProductVariantColor.ColorName
                            .ToLower()
                            .Contains(term, StringComparison.CurrentCultureIgnoreCase)));
        }

        return query.Select(x => new InventoryReportSummaryRowResponse
            {
                ProductId = x.ProductVariant!.ProductId,
                ProductName = x.ProductVariant.Product!.Name,
                VariantId = x.ProductVariant.Id,
                VariantName = x.ProductVariant.VariantName,
                ColorId = x.ProductVariantColorId,
                ColorName = x.ProductVariantColor != null ? x.ProductVariantColor.ColorName : null,
                ImportedQty = x.ImportedQty,
                ExportedQty = x.ExportedQty,
                StockQty = x.StockQty,
                OrderedQty = x.OrderedQty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryReportSummaryPageResponse> GetInventoryReportSummaryAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var allData = await GetInventoryReportSummaryRowsAsync(searchTerm, cancellationToken);

        var totalCount = allData.Select(x => x.ProductId).Distinct().Count();
        var paginatedProductIds = allData.Select(x => x.ProductId)
            .Distinct()
            .OrderBy(id => id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var groupedByProduct = allData
            .Where(x => paginatedProductIds.Contains(x.ProductId))
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(
                gp => new InventoryReportSummaryResponse
                {
                    ProductId = gp.Key.ProductId,
                    ProductName = gp.Key.ProductName,
                    ImportedQty = gp.Sum(x => x.ImportedQty),
                    ExportedQty = gp.Sum(x => x.ExportedQty),
                    InventoryQty = gp.Sum(x => x.StockQty),
                    OrderedQty = gp.Sum(x => x.OrderedQty),
                    RemainingQty = gp.Sum(x => x.StockQty) - gp.Sum(x => x.OrderedQty),
                    Variants =
                        [.. gp.GroupBy(x => new { x.VariantId, x.VariantName })
                                .Select(
                                    gv => new InventoryReportVariantResponse
                                    {
                                        VariantId = gv.Key.VariantId,
                                        VariantName = gv.Key.VariantName,
                                        ImportedQty = gv.Sum(x => x.ImportedQty),
                                        ExportedQty = gv.Sum(x => x.ExportedQty),
                                        InventoryQty = gv.Sum(x => x.StockQty),
                                        OrderedQty = gv.Sum(x => x.OrderedQty),
                                        RemainingQty = gv.Sum(x => x.StockQty) - gv.Sum(x => x.OrderedQty),
                                        VariantColors =
                                            [.. gv.Where(x => x.ColorId != null)
                                                    .Select(
                                                        gc => new InventoryReportColorResponse
                                                        {
                                                            ColorId = gc.ColorId!.Value,
                                                            ColorName = gc.ColorName,
                                                            ImportedQty = gc.ImportedQty,
                                                            ExportedQty = gc.ExportedQty,
                                                            InventoryQty = gc.StockQty,
                                                            OrderedQty = gc.OrderedQty,
                                                            RemainingQty = gc.StockQty - gc.OrderedQty
                                                        })]
                                    })]
                })
            .ToList();

        return new InventoryReportSummaryPageResponse(
            groupedByProduct,
            totalCount,
            pageNumber,
            pageSize);
    }
}
