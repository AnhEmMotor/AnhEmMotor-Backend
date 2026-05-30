using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Primitives;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportSummary
{
    public sealed class GetInventoryReportSummaryQueryHandler(
        IProductReadRepository productRepository,
        IInventoryReceiptReadRepository receiptRepository)
        : IRequestHandler<GetInventoryReportSummaryQuery, Result<PagedResult<InventoryReportSummaryResponse>>>
    {
        public async Task<Result<PagedResult<InventoryReportSummaryResponse>>> Handle(
            GetInventoryReportSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var products = await productRepository.GetAllProductsWithInventoryDetailsAsync(cancellationToken).ConfigureAwait(false);
            var receipts = await receiptRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var finishedReceiptInfos = receipts
                .Where(r => InventoryReceiptStatus.IsFinished(r.StatusId))
                .SelectMany(r => r.InventoryReceiptInfos)
                .ToList();

            var report = new List<InventoryReportSummaryResponse>();

            foreach (var product in products)
            {
                var productVariantsList = new List<InventoryReportVariantResponse>();

                foreach (var variant in product.ProductVariants)
                {
                    if (variant.ProductVariantColors != null && variant.ProductVariantColors.Count != 0)
                    {
                        var variantColorsList = new List<InventoryReportColorResponse>();

                        foreach (var color in variant.ProductVariantColors)
                        {
                            int imported = finishedReceiptInfos
                                .Where(ii => (ii.QuotationProductRow != null && ii.QuotationProductRow.ProductVariantId == variant.Id && ii.QuotationProductRow.ProductVariantColorId == color.Id) ||
                                             (ii.PurchaseRequestItem != null && ii.PurchaseRequestItem.ProductVariantId == variant.Id && ii.PurchaseRequestItem.ProductVariantColorId == color.Id))
                                .Sum(ii => ii.Count ?? 0);

                            int exported = variant.OutputInfos
                                .Where(oi => oi.OutputOrder != null &&
                                             string.Equals(oi.OutputOrder.StatusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase) &&
                                             oi.ProductVariantColorId == color.Id)
                                .Sum(oi => oi.Count ?? 0);

                            int ordered = variant.OutputInfos
                                .Where(oi => oi.OutputOrder != null &&
                                             OrderStatus.IsBookingStatus(oi.OutputOrder.StatusId) &&
                                             oi.ProductVariantColorId == color.Id)
                                .Sum(oi => oi.Count ?? 0);

                            int inventory = imported - exported;
                            int remaining = inventory - ordered;

                            variantColorsList.Add(new InventoryReportColorResponse
                            {
                                ColorId = color.Id,
                                ColorName = color.ColorName,
                                ImportedQty = imported,
                                ExportedQty = exported,
                                InventoryQty = inventory,
                                OrderedQty = ordered,
                                RemainingQty = remaining
                            });
                        }

                        productVariantsList.Add(new InventoryReportVariantResponse
                        {
                            VariantId = variant.Id,
                            VariantName = variant.VariantName,
                            ImportedQty = variantColorsList.Sum(c => c.ImportedQty),
                            ExportedQty = variantColorsList.Sum(c => c.ExportedQty),
                            InventoryQty = variantColorsList.Sum(c => c.InventoryQty),
                            OrderedQty = variantColorsList.Sum(c => c.OrderedQty),
                            RemainingQty = variantColorsList.Sum(c => c.RemainingQty),
                            VariantColors = variantColorsList
                        });
                    }
                    else
                    {
                        int imported = finishedReceiptInfos
                            .Where(ii => (ii.QuotationProductRow != null && ii.QuotationProductRow.ProductVariantId == variant.Id) ||
                                         (ii.PurchaseRequestItem != null && ii.PurchaseRequestItem.ProductVariantId == variant.Id))
                            .Sum(ii => ii.Count ?? 0);

                        int exported = variant.OutputInfos
                            .Where(oi => oi.OutputOrder != null &&
                                         string.Equals(oi.OutputOrder.StatusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase))
                            .Sum(oi => oi.Count ?? 0);

                        int ordered = variant.OutputInfos
                            .Where(oi => oi.OutputOrder != null &&
                                         OrderStatus.IsBookingStatus(oi.OutputOrder.StatusId))
                            .Sum(oi => oi.Count ?? 0);

                        int inventory = imported - exported;
                        int remaining = inventory - ordered;

                        productVariantsList.Add(new InventoryReportVariantResponse
                        {
                            VariantId = variant.Id,
                            VariantName = variant.VariantName,
                            ImportedQty = imported,
                            ExportedQty = exported,
                            InventoryQty = inventory,
                            OrderedQty = ordered,
                            RemainingQty = remaining,
                            VariantColors = null
                        });
                    }
                }

                report.Add(new InventoryReportSummaryResponse
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImportedQty = productVariantsList.Sum(v => v.ImportedQty),
                    ExportedQty = productVariantsList.Sum(v => v.ExportedQty),
                    InventoryQty = productVariantsList.Sum(v => v.InventoryQty),
                    OrderedQty = productVariantsList.Sum(v => v.OrderedQty),
                    RemainingQty = productVariantsList.Sum(v => v.RemainingQty),
                    Variants = productVariantsList
                });
            }

            // Lọc kết quả theo tìm kiếm 3 cấp độ
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                var filteredReport = new List<InventoryReportSummaryResponse>();

                foreach (var product in report)
                {
                    bool productMatches = product.ProductName != null && product.ProductName.ToLower().Contains(search);
                    var matchedVariants = new List<InventoryReportVariantResponse>();

                    foreach (var variant in product.Variants)
                    {
                        bool variantMatches = variant.VariantName != null && variant.VariantName.ToLower().Contains(search);

                        if (variant.VariantColors != null && variant.VariantColors.Any())
                        {
                            var matchedColors = variant.VariantColors
                                .Where(c => c.ColorName != null && c.ColorName.ToLower().Contains(search))
                                .ToList();

                            if (productMatches || variantMatches)
                            {
                                matchedVariants.Add(new InventoryReportVariantResponse
                                {
                                    VariantId = variant.VariantId,
                                    VariantName = variant.VariantName,
                                    ImportedQty = variant.ImportedQty,
                                    ExportedQty = variant.ExportedQty,
                                    InventoryQty = variant.InventoryQty,
                                    OrderedQty = variant.OrderedQty,
                                    RemainingQty = variant.RemainingQty,
                                    VariantColors = variant.VariantColors
                                });
                            }
                            else if (matchedColors.Any())
                            {
                                matchedVariants.Add(new InventoryReportVariantResponse
                                {
                                    VariantId = variant.VariantId,
                                    VariantName = variant.VariantName,
                                    ImportedQty = matchedColors.Sum(c => c.ImportedQty),
                                    ExportedQty = matchedColors.Sum(c => c.ExportedQty),
                                    InventoryQty = matchedColors.Sum(c => c.InventoryQty),
                                    OrderedQty = matchedColors.Sum(c => c.OrderedQty),
                                    RemainingQty = matchedColors.Sum(c => c.RemainingQty),
                                    VariantColors = matchedColors
                                });
                            }
                        }
                        else
                        {
                            if (productMatches || variantMatches)
                            {
                                matchedVariants.Add(variant);
                            }
                        }
                    }

                    if (productMatches || matchedVariants.Any())
                    {
                        var finalVariants = matchedVariants.Any() ? matchedVariants : product.Variants;
                        filteredReport.Add(new InventoryReportSummaryResponse
                        {
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            ImportedQty = finalVariants.Sum(v => v.ImportedQty),
                            ExportedQty = finalVariants.Sum(v => v.ExportedQty),
                            InventoryQty = finalVariants.Sum(v => v.InventoryQty),
                            OrderedQty = finalVariants.Sum(v => v.OrderedQty),
                            RemainingQty = finalVariants.Sum(v => v.RemainingQty),
                            Variants = finalVariants
                        });
                    }
                }

                report = filteredReport;
            }

            // Phân trang
            int pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 10;
            long totalCount = report.Count;

            var paginatedItems = report
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<InventoryReportSummaryResponse>(
                paginatedItems,
                totalCount,
                pageNumber,
                pageSize);

            return Result<PagedResult<InventoryReportSummaryResponse>>.Success(result);
        }
    }
}

