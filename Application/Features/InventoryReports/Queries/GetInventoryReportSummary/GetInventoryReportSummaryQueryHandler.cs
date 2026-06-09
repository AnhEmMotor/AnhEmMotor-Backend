using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryOnHand;
using Domain.Primitives;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportSummary
{
    public sealed class GetInventoryReportSummaryQueryHandler(IInventoryOnHandReadRepository readRepository) : IRequestHandler<GetInventoryReportSummaryQuery, Result<PagedResult<InventoryReportSummaryResponse>>>
    {
        public async Task<Result<PagedResult<InventoryReportSummaryResponse>>> Handle(
            GetInventoryReportSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var query = readRepository.GetQuery()
                .Where(x => x.ProductVariant != null && x.ProductVariant.Product != null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(x => 
                    (x.ProductVariant!.Product!.Name != null && x.ProductVariant.Product.Name.ToLower().Contains(term)) ||
                    (x.ProductVariant.VariantName != null && x.ProductVariant.VariantName.ToLower().Contains(term)) ||
                    (x.ProductVariantColor != null && x.ProductVariantColor.ColorName != null && x.ProductVariantColor.ColorName.ToLower().Contains(term)));
            }

            var allData = query.Select(x => new 
            {
                x.ProductVariant!.ProductId,
                ProductName = x.ProductVariant.Product!.Name,
                VariantId = x.ProductVariant.Id,
                VariantName = x.ProductVariant.VariantName,
                ColorId = x.ProductVariantColorId,
                ColorName = x.ProductVariantColor != null ? x.ProductVariantColor.ColorName : null,
                x.ImportedQty,
                x.ExportedQty,
                x.StockQty,
                x.OrderedQty
            }).ToList();

            var totalCount = allData.Select(x => x.ProductId).Distinct().Count();

            var paginatedProductIds = allData.Select(x => x.ProductId).Distinct()
                .OrderBy(id => id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var rawData = allData.Where(x => paginatedProductIds.Contains(x.ProductId)).ToList();

            var groupedByProduct = rawData
                .GroupBy(x => new { x.ProductId, x.ProductName })
                .Select(gp => new InventoryReportSummaryResponse
                {
                    ProductId = gp.Key.ProductId,
                    ProductName = gp.Key.ProductName,
                    ImportedQty = gp.Sum(x => x.ImportedQty),
                    ExportedQty = gp.Sum(x => x.ExportedQty),
                    InventoryQty = gp.Sum(x => x.StockQty),
                    OrderedQty = gp.Sum(x => x.OrderedQty),
                    RemainingQty = gp.Sum(x => x.StockQty) - gp.Sum(x => x.OrderedQty),
                    Variants = gp.GroupBy(x => new { x.VariantId, x.VariantName })
                                 .Select(gv => new InventoryReportVariantResponse
                                 {
                                     VariantId = gv.Key.VariantId,
                                     VariantName = gv.Key.VariantName,
                                     ImportedQty = gv.Sum(x => x.ImportedQty),
                                     ExportedQty = gv.Sum(x => x.ExportedQty),
                                     InventoryQty = gv.Sum(x => x.StockQty),
                                     OrderedQty = gv.Sum(x => x.OrderedQty),
                                     RemainingQty = gv.Sum(x => x.StockQty) - gv.Sum(x => x.OrderedQty),
                                     VariantColors = gv.Where(x => x.ColorId != null)
                                                       .Select(gc => new InventoryReportColorResponse
                                                       {
                                                           ColorId = gc.ColorId!.Value,
                                                           ColorName = gc.ColorName,
                                                           ImportedQty = gc.ImportedQty,
                                                           ExportedQty = gc.ExportedQty,
                                                           InventoryQty = gc.StockQty,
                                                           OrderedQty = gc.OrderedQty,
                                                           RemainingQty = gc.StockQty - gc.OrderedQty
                                                       }).ToList()
                                 }).ToList()
                }).ToList();

            var pagedResult = new PagedResult<InventoryReportSummaryResponse>(groupedByProduct, totalCount, request.PageNumber, request.PageSize);
            return pagedResult;
        }
    }
}
