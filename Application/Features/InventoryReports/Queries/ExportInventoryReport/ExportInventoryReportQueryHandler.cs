using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.InventoryReceipt;
using ClosedXML.Excel;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Primitives;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.ExportInventoryReport
{
    public sealed class ExportInventoryReportQueryHandler(
        IProductReadRepository productRepository,
        IInventoryReceiptReadRepository receiptRepository)
        : IRequestHandler<ExportInventoryReportQuery, Result<FileStreamResult>>
    {
        public async Task<Result<FileStreamResult>> Handle(
            ExportInventoryReportQuery request,
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

            // Tạo workbook Excel
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo tồn kho");
            worksheet.Row(1).Height = 40;
            worksheet.Row(2).Height = 20;
            worksheet.Row(3).Height = 15;
            worksheet.Row(4).Height = 30;

            worksheet.Cell("A1").Value = "BÁO CÁO XUẤT - NHẬP - TỒN KHO";
            var titleRange = worksheet.Range("A1:G1");
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
            titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
            var subtitleRange = worksheet.Range("A2:G2");
            subtitleRange.Merge();
            subtitleRange.Style.Font.Italic = true;
            subtitleRange.Style.Font.FontSize = 10;
            subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            string[] headers =
            {
                "STT",
                "Tên sản phẩm / Biến thể / Màu sắc",
                "Số lượng đã nhập",
                "Số lượng đã xuất",
                "Số lượng tồn kho",
                "Số lượng đã đặt",
                "Số lượng còn lại"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            worksheet.Column(1).Width = 6;
            worksheet.Column(2).Width = 45;
            worksheet.Column(3).Width = 18;
            worksheet.Column(4).Width = 18;
            worksheet.Column(5).Width = 18;
            worksheet.Column(6).Width = 18;
            worksheet.Column(7).Width = 18;

            int rowIndex = 5;
            int stt = 1;

            foreach (var product in report)
            {
                // Hàng Sản phẩm
                worksheet.Row(rowIndex).Height = 24;
                worksheet.Cell(rowIndex, 1).Value = stt++;
                worksheet.Cell(rowIndex, 2).Value = product.ProductName ?? string.Empty;
                worksheet.Cell(rowIndex, 3).Value = product.ImportedQty;
                worksheet.Cell(rowIndex, 4).Value = product.ExportedQty;
                worksheet.Cell(rowIndex, 5).Value = product.InventoryQty;
                worksheet.Cell(rowIndex, 6).Value = product.OrderedQty;
                worksheet.Cell(rowIndex, 7).Value = product.RemainingQty;

                // Style cho sản phẩm (in đậm, nền hơi xám nhẹ)
                for (int col = 1; col <= 7; col++)
                {
                    var cell = worksheet.Cell(rowIndex, col);
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#ECEFF1"));
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    if (col != 2) cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                rowIndex++;

                foreach (var variant in product.Variants)
                {
                    // Hàng Biến thể
                    worksheet.Row(rowIndex).Height = 24;
                    worksheet.Cell(rowIndex, 1).Value = string.Empty;
                    worksheet.Cell(rowIndex, 2).Value = $"   - {variant.VariantName}";
                    worksheet.Cell(rowIndex, 3).Value = variant.ImportedQty;
                    worksheet.Cell(rowIndex, 4).Value = variant.ExportedQty;
                    worksheet.Cell(rowIndex, 5).Value = variant.InventoryQty;
                    worksheet.Cell(rowIndex, 6).Value = variant.OrderedQty;
                    worksheet.Cell(rowIndex, 7).Value = variant.RemainingQty;

                    for (int col = 1; col <= 7; col++)
                    {
                        var cell = worksheet.Cell(rowIndex, col);
                        cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        if (col != 2) cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // Nền cho hàng biến thể nhạt hơn nữa
                        cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F7F8"));
                    }
                    rowIndex++;

                    if (variant.VariantColors != null)
                    {
                        foreach (var color in variant.VariantColors)
                        {
                            // Hàng Màu sắc
                            worksheet.Row(rowIndex).Height = 24;
                            worksheet.Cell(rowIndex, 1).Value = string.Empty;
                            worksheet.Cell(rowIndex, 2).Value = $"       + {color.ColorName}";
                            worksheet.Cell(rowIndex, 3).Value = color.ImportedQty;
                            worksheet.Cell(rowIndex, 4).Value = color.ExportedQty;
                            worksheet.Cell(rowIndex, 5).Value = color.InventoryQty;
                            worksheet.Cell(rowIndex, 6).Value = color.OrderedQty;
                            worksheet.Cell(rowIndex, 7).Value = color.RemainingQty;

                            for (int col = 1; col <= 7; col++)
                            {
                                var cell = worksheet.Cell(rowIndex, col);
                                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                                if (col != 2) cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                cell.Style.Font.Italic = true;
                            }
                            rowIndex++;
                        }
                    }
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var fileResult = new FileStreamResult(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Bao_cao_ton_kho.xlsx");

            return Result<FileStreamResult>.Success(fileResult);
        }
    }
}
