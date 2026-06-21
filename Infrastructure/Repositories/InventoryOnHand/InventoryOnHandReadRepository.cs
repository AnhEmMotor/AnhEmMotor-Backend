using Application.ApiContracts.InventoryReport.Responses;
using Application.Interfaces.Repositories.InventoryOnHand;
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
        int? month,
        int? year,
        CancellationToken cancellationToken)
    {
        var targetMonth = month ?? DateTimeOffset.UtcNow.Month;
        var targetYear = year ?? DateTimeOffset.UtcNow.Year;

        return context.InventoryOnHands
            .FirstOrDefaultAsync(
                x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId && x.Month == targetMonth && x.Year == targetYear,
                cancellationToken);
    }

    public Task<List<InventoryReportSummaryRowResponse>> GetInventoryReportSummaryRowsAsync(
        string? searchTerm,
        int? month,
        int? year,
        CancellationToken cancellationToken)
    {
        var targetMonth = month ?? DateTimeOffset.UtcNow.Month;
        var targetYear = year ?? DateTimeOffset.UtcNow.Year;

        var query = context.InventoryOnHands
            .AsNoTracking()
            .Where(x => x.ProductVariant != null && x.ProductVariant.Product != null && x.Month == targetMonth && x.Year == targetYear);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(
                x => (x.ProductVariant!.Product!.Name != null &&
                        x.ProductVariant.Product.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.ProductVariant.VariantName != null &&
                        x.ProductVariant.VariantName.ToLower().Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                    (x.ProductVariantColor != null &&
                        x.ProductVariantColor.ColorName != null &&
                        x.ProductVariantColor.ColorName
                            .ToLower()
                            .Contains(term, StringComparison.CurrentCultureIgnoreCase)));
        }
        return query.Select(
            x => new InventoryReportSummaryRowResponse
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
                OrderedQty = x.OrderedQty,
                BeginningQty = x.BeginningQty // assuming we want to add this to response later, but for now leave as is if not in DTO
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryReportSummaryPageResponse> GetInventoryReportSummaryAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        int? month,
        int? year,
        CancellationToken cancellationToken)
    {
        var allData = await GetInventoryReportSummaryRowsAsync(searchTerm, month, year, cancellationToken).ConfigureAwait(false);
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
                    BeginningQty = gp.Sum(x => x.BeginningQty),
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
                                    BeginningQty = gv.Sum(x => x.BeginningQty),
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
                                                            BeginningQty = gc.BeginningQty,
                                                            ImportedQty = gc.ImportedQty,
                                                            ExportedQty = gc.ExportedQty,
                                                            InventoryQty = gc.StockQty,
                                                            OrderedQty = gc.OrderedQty,
                                                            RemainingQty = gc.StockQty - gc.OrderedQty
                                                        })]
                                })]
                })
            .ToList();
        return new InventoryReportSummaryPageResponse(groupedByProduct, totalCount, pageNumber, pageSize);
    }
}
