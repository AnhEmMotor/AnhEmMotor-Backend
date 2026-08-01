using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Excel;

public class PurchaseRequestExcelService : IPurchaseRequestExcelService
{
    public byte[] ExportPurchaseRequests(
        IReadOnlyList<PurchaseRequestListResponse> requests,
        IReadOnlyList<PurchaseRequestItem> items,
        IReadOnlyDictionary<int, string> supplierNames)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Yêu cầu mua hàng");
        worksheet.Cell("A1").Value = "MẪU NHẬP YÊU CẦU MUA HÀNG";
        var titleRange = worksheet.Range("A1:G1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các mặt hàng có chung [Mã phiếu] sẽ được gộp thành 1 phiếu.";
        var subtitleRange = worksheet.Range("A2:G2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers =
        {
            "Mã phiếu",
            "Ghi chú",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm (nếu có)",
            "Số lượng yêu cầu",
            "Tên nhà cung cấp"
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
        foreach (var r in requests)
        {
            var rItems = items.Where(x => x.PurchaseRequestId == r.Id).ToList();
            if (rItems.Count == 0)
            {
                worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                worksheet.Cell(rowIndex, 2).Value = r.Note ?? string.Empty;
                rowIndex++;
            } else
            {
                foreach (var item in rItems)
                {
                    worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                    worksheet.Cell(rowIndex, 2).Value = r.Note ?? string.Empty;
                    worksheet.Cell(rowIndex, 3).Value = item.ProductVariant?.Product?.Name ?? string.Empty;
                    worksheet.Cell(rowIndex, 4).Value = item.ProductVariant?.VariantName ?? string.Empty;
                    worksheet.Cell(rowIndex, 5).Value = item.ProductVariantColor?.ColorName ?? string.Empty;
                    worksheet.Cell(rowIndex, 6).Value = item.Quantity;
                    string supplierName = string.Empty;
                    if (item.SupplierId.HasValue && supplierNames.TryGetValue(item.SupplierId.Value, out var sName))
                    {
                        supplierName = sName;
                    }
                    worksheet.Cell(rowIndex, 7).Value = supplierName;
                    rowIndex++;
                }
            }
        }
        worksheet.Columns().AdjustToContents();
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 30;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 40;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 30;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] BuildImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thêm YCMH");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "MẪU NHẬP YÊU CẦU MUA HÀNG";
        var titleRange = worksheet.Range("A1:G1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các mặt hàng có chung [Mã phiếu] sẽ được gộp thành 1 phiếu.";
        var subtitleRange = worksheet.Range("A2:G2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers =
        {
            "Mã phiếu",
            "Ghi chú",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm (nếu có)",
            "Số lượng yêu cầu",
            "Tên nhà cung cấp"
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
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 30;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 40;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 30;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public IReadOnlyList<PurchaseRequestImportRow>? ParseImportRows(byte[] fileBytes)
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
        var rows = new List<PurchaseRequestImportRow>();
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var tempCode = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            var note = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var productName = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var variantName = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            var colorName = row.Cell(5).GetString()?.Trim() ?? string.Empty;
            var qtyStr = row.Cell(6).GetString()?.Trim() ?? string.Empty;
            var supplierName = row.Cell(7).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tempCode) && string.IsNullOrWhiteSpace(productName))
            {
                continue;
            }
            rows.Add(new PurchaseRequestImportRow(tempCode, note, productName, variantName, colorName, qtyStr, supplierName));
        }
        return rows;
    }

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(IReadOnlyList<PurchaseRequestImportFailedRow> failedRows)
    {
        string[] headers1 =
        {
            "Mã phiếu",
            "Ghi chú",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm (nếu có)",
            "Số lượng yêu cầu",
            "Tên nhà cung cấp"
        };
        using var wb1 = new XLWorkbook();
        var ws1 = wb1.Worksheets.Add("Lỗi nhập");
        for (int i = 0; i < headers1.Length; i++)
            ws1.Cell(1, i + 1).Value = headers1[i];
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws1.Cell(i + 2, 1).Value = failedRows[i].TempCode;
            ws1.Cell(i + 2, 2).Value = failedRows[i].Note;
            ws1.Cell(i + 2, 3).Value = failedRows[i].ProductName;
            ws1.Cell(i + 2, 4).Value = failedRows[i].VariantName;
            ws1.Cell(i + 2, 5).Value = failedRows[i].ColorName;
            ws1.Cell(i + 2, 6).Value = failedRows[i].Qty;
            ws1.Cell(i + 2, 7).Value = failedRows[i].SupplierName;
        }
        using var stream1 = new MemoryStream();
        wb1.SaveAs(stream1);

        using var wb2 = new XLWorkbook();
        var ws2 = wb2.Worksheets.Add("Lỗi nhập");
        for (int i = 0; i < headers1.Length; i++)
            ws2.Cell(1, i + 1).Value = headers1[i];
        ws2.Cell(1, headers1.Length + 1).Value = "Lý do lỗi";
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws2.Cell(i + 2, 1).Value = failedRows[i].TempCode;
            ws2.Cell(i + 2, 2).Value = failedRows[i].Note;
            ws2.Cell(i + 2, 3).Value = failedRows[i].ProductName;
            ws2.Cell(i + 2, 4).Value = failedRows[i].VariantName;
            ws2.Cell(i + 2, 5).Value = failedRows[i].ColorName;
            ws2.Cell(i + 2, 6).Value = failedRows[i].Qty;
            ws2.Cell(i + 2, 7).Value = failedRows[i].SupplierName;
            ws2.Cell(i + 2, 8).Value = failedRows[i].Reason;
        }
        using var stream2 = new MemoryStream();
        wb2.SaveAs(stream2);

        return (stream1.ToArray(), stream2.ToArray());
    }
}
