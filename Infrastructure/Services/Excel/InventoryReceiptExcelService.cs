using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Excel;

public class InventoryReceiptExcelService : IInventoryReceiptExcelService
{
    public byte[] ExportInventoryReceipts(IReadOnlyList<InventoryReceiptListResponse> receipts, IReadOnlyList<InventoryReceiptInfo> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Phiếu nhập");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "DANH SÁCH PHIẾU NHẬP KHO";
        var titleRange = worksheet.Range("A1:J1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers =
        {
            "Mã phiếu",
            "Ghi chú",
            "ID Yêu cầu mua hàng",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm",
            "Số lượng",
            "Số khung",
            "Số máy",
            "Trạng thái"
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
        int rowIndex = 5;
        foreach (var r in receipts)
        {
            var statusName = r.StatusId switch
            {
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Draft => "Phiếu tạm",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent => "Đã gửi",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve => "Đã duyệt",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Reject => "Đã từ chối",
                _ => r.StatusId ?? string.Empty
            };
            var rItems = items.Where(x => x.InventoryReceiptId == r.Id).ToList();
            if (rItems.Count == 0)
            {
                worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                worksheet.Cell(rowIndex, 10).Value = statusName;
                rowIndex++;
            } else
            {
                foreach (var item in rItems)
                {
                    var prId = item.PurchaseRequestItem?.PurchaseRequestId.ToString() ?? string.Empty;
                    var productName = item.PurchaseRequestItem?.ProductVariant?.Product?.Name ?? string.Empty;
                    var variantName = item.PurchaseRequestItem?.ProductVariant?.VariantName ?? string.Empty;
                    var colorName = item.PurchaseRequestItem?.ProductVariantColor?.ColorName ?? string.Empty;
                    if (item.Vehicles != null && item.Vehicles.Any())
                    {
                        foreach (var v in item.Vehicles)
                        {
                            worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                            worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                            worksheet.Cell(rowIndex, 3).Value = prId;
                            worksheet.Cell(rowIndex, 4).Value = productName;
                            worksheet.Cell(rowIndex, 5).Value = variantName;
                            worksheet.Cell(rowIndex, 6).Value = colorName;
                            worksheet.Cell(rowIndex, 7).Value = item.Count ?? 0;
                            worksheet.Cell(rowIndex, 8).Value = v.VinNumber ?? string.Empty;
                            worksheet.Cell(rowIndex, 9).Value = v.EngineNumber ?? string.Empty;
                            worksheet.Cell(rowIndex, 10).Value = statusName;
                            rowIndex++;
                        }
                    } else
                    {
                        worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                        worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                        worksheet.Cell(rowIndex, 3).Value = prId;
                        worksheet.Cell(rowIndex, 4).Value = productName;
                        worksheet.Cell(rowIndex, 5).Value = variantName;
                        worksheet.Cell(rowIndex, 6).Value = colorName;
                        worksheet.Cell(rowIndex, 7).Value = item.Count ?? 0;
                        worksheet.Cell(rowIndex, 8).Value = string.Empty;
                        worksheet.Cell(rowIndex, 9).Value = string.Empty;
                        worksheet.Cell(rowIndex, 10).Value = statusName;
                        rowIndex++;
                    }
                }
            }
        }
        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] BuildImportTemplate(IReadOnlyList<PurchaseRequestItem> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thêm Phiếu Nhập");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "MẪU NHẬP PHIẾU NHẬP KHO (THEO MẶT HÀNG)";
        var titleRange = worksheet.Range("A1:H1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các ô tô màu vàng là các ô cần nhập thông tin.";
        var subtitleRange = worksheet.Range("A2:H2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A3").Value = "Ghi chú cho toàn bộ phiếu:";
        worksheet.Cell("A3").Style.Font.Bold = true;
        worksheet.Cell("A3").Style.Font.Italic = true;
        worksheet.Cell("A3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        var noteRange = worksheet.Range("B3:H3");
        noteRange.Merge();
        noteRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        string[] headers =
        {
            "Mã yêu cầu mua hàng",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm",
            "Số lượng còn lại",
            "Số lượng nhập",
            "Số khung",
            "Số máy"
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
        worksheet.Column(1).Width = 25;
        worksheet.Column(2).Width = 40;
        worksheet.Column(3).Width = 25;
        worksheet.Column(4).Width = 25;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 20;
        int currentRow = 5;
        foreach (var item in items)
        {
            int receivedCount = item.InventoryReceiptInfos?.Sum(iri => iri.Count ?? 0) ?? 0;
            int remainingCount = item.Quantity - receivedCount;
            if (remainingCount <= 0)
                continue;
            string managementType = item.ProductVariant?.Product?.ProductCategory?.ManagementType ?? "sku";
            if (managementType == "vin_number")
            {
                for (int j = 0; j < remainingCount; j++)
                {
                    FillRowData(worksheet, currentRow, item, remainingCount, j == 0);
                    worksheet.Cell(currentRow, 7).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    worksheet.Cell(currentRow, 8).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    currentRow++;
                }
            } else
            {
                FillRowData(worksheet, currentRow, item, remainingCount, true);
                worksheet.Cell(currentRow, 6).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                currentRow++;
            }
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void FillRowData(
        IXLWorksheet worksheet,
        int row,
        PurchaseRequestItem item,
        int remainingCount,
        bool showRemainingCount)
    {
        worksheet.Cell(row, 1).Value = item.PurchaseRequestId;
        worksheet.Cell(row, 2).Value = item.ProductVariant?.Product?.Name ?? string.Empty;
        worksheet.Cell(row, 3).Value = item.ProductVariant?.VariantName ?? string.Empty;
        worksheet.Cell(row, 4).Value = item.ProductVariantColor?.ColorName ?? string.Empty;
        if (showRemainingCount)
        {
            worksheet.Cell(row, 5).Value = remainingCount;
        } else
        {
            worksheet.Cell(row, 5).Value = string.Empty;
        }
    }

    public IReadOnlyList<InventoryReceiptImportRow>? ParseImportRows(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return null;
        }
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return [];
        }
        var parsedRows = new List<InventoryReceiptImportRow>();
        var generalNote = worksheet.Cell(3, 2).GetString()?.Trim() ?? string.Empty;
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var prIdStr = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            var productName = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var variantName = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var colorName = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            var remQtyStr = row.Cell(5).GetString()?.Trim() ?? string.Empty;
            var qtyStr = row.Cell(6).GetString()?.Trim() ?? string.Empty;
            var vin = row.Cell(7).GetString()?.Trim() ?? string.Empty;
            var engine = row.Cell(8).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(qtyStr) && string.IsNullOrWhiteSpace(vin) && string.IsNullOrWhiteSpace(engine))
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(qtyStr) &&
                (!string.IsNullOrWhiteSpace(vin) || !string.IsNullOrWhiteSpace(engine)))
            {
                qtyStr = "1";
            }
            if (string.IsNullOrWhiteSpace(prIdStr) && string.IsNullOrWhiteSpace(productName))
            {
                continue;
            }
            parsedRows.Add(
                new InventoryReceiptImportRow(prIdStr, productName, variantName, colorName, remQtyStr, qtyStr, vin, engine, generalNote));
        }
        return parsedRows;
    }

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(IReadOnlyList<InventoryReceiptImportFailedRow> failedRows)
    {
        var generalNote = failedRows.Count > 0 ? failedRows[0].Note : string.Empty;
        string[] headers1 =
        {
            "Mã yêu cầu mua hàng",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm",
            "Số lượng còn lại",
            "Số lượng nhập",
            "Số khung",
            "Số máy"
        };
        Action<IXLWorksheet> addTitleRows = (ws) =>
        {
            ws.Row(1).Height = 40;
            ws.Row(2).Height = 20;
            ws.Row(3).Height = 15;
            ws.Row(4).Height = 30;
            ws.Cell("A1").Value = "MẪU NHẬP PHIẾU NHẬP KHO (THEO MẶT HÀNG)";
            var titleRange = ws.Range("A1:H1");
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
            titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các ô tô màu vàng là các ô cần nhập thông tin.";
            var subtitleRange = ws.Range("A2:H2");
            subtitleRange.Merge();
            subtitleRange.Style.Font.Italic = true;
            subtitleRange.Style.Font.FontSize = 10;
            subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
            subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell("A3").Value = "Ghi chú cho toàn bộ phiếu:";
            ws.Cell("A3").Style.Font.Bold = true;
            ws.Cell("A3").Style.Font.Italic = true;
            ws.Cell("A3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell("B3").Value = generalNote;
            var noteRange = ws.Range("B3:H3");
            noteRange.Merge();
            noteRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            for (int i = 0; i < headers1.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers1[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Column(1).Width = 25;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 25;
            ws.Column(4).Width = 25;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 20;
            ws.Column(7).Width = 20;
            ws.Column(8).Width = 20;
        };
        byte[] withoutReasonBytes;
        using (var wb1 = new XLWorkbook())
        {
            var ws1 = wb1.Worksheets.Add("Lỗi nhập");
            addTitleRows(ws1);
            for (int i = 0; i < failedRows.Count; i++)
            {
                ws1.Cell(i + 5, 1).Value = failedRows[i].PrId;
                ws1.Cell(i + 5, 2).Value = failedRows[i].ProductName;
                ws1.Cell(i + 5, 3).Value = failedRows[i].VariantName;
                ws1.Cell(i + 5, 4).Value = failedRows[i].ColorName;
                ws1.Cell(i + 5, 5).Value = failedRows[i].RemQtyStr;
                ws1.Cell(i + 5, 6).Value = failedRows[i].Qty;
                ws1.Cell(i + 5, 7).Value = failedRows[i].Vin;
                ws1.Cell(i + 5, 8).Value = failedRows[i].Engine;
            }
            using var stream1 = new MemoryStream();
            wb1.SaveAs(stream1);
            withoutReasonBytes = stream1.ToArray();
        }
        byte[] withReasonBytes;
        using (var wb2 = new XLWorkbook())
        {
            var ws2 = wb2.Worksheets.Add("Lỗi nhập chi tiết");
            addTitleRows(ws2);
            var cellReason = ws2.Cell(4, 9);
            cellReason.Value = "Lý do lỗi";
            cellReason.Style.Font.Bold = true;
            cellReason.Style.Font.FontColor = XLColor.White;
            cellReason.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E53935"));
            cellReason.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cellReason.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cellReason.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            ws2.Column(9).Width = 40;
            for (int i = 0; i < failedRows.Count; i++)
            {
                ws2.Cell(i + 5, 1).Value = failedRows[i].PrId;
                ws2.Cell(i + 5, 2).Value = failedRows[i].ProductName;
                ws2.Cell(i + 5, 3).Value = failedRows[i].VariantName;
                ws2.Cell(i + 5, 4).Value = failedRows[i].ColorName;
                ws2.Cell(i + 5, 5).Value = failedRows[i].RemQtyStr;
                ws2.Cell(i + 5, 6).Value = failedRows[i].Qty;
                ws2.Cell(i + 5, 7).Value = failedRows[i].Vin;
                ws2.Cell(i + 5, 8).Value = failedRows[i].Engine;
                ws2.Cell(i + 5, 9).Value = failedRows[i].Reason;
            }
            using var stream2 = new MemoryStream();
            wb2.SaveAs(stream2);
            withReasonBytes = stream2.ToArray();
        }
        return (withoutReasonBytes, withReasonBytes);
    }
}
