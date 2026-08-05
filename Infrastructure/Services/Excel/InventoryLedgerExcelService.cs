using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Excel;

public class InventoryLedgerExcelService : IInventoryLedgerExcelService
{
    public byte[] ExportInventoryLedger(IReadOnlyList<InventoryLedger> entries, DateTime? startDate, DateTime? endDate)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sổ cái tồn kho");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "SỔ CÁI TỒN KHO";
        var titleRange = worksheet.Range("A1:K1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string periodInfo = "Tất cả";
        if (startDate.HasValue && endDate.HasValue)
        {
            periodInfo = $"Từ {startDate.Value:dd/MM/yyyy} đến {endDate.Value:dd/MM/yyyy}";
        } else if (startDate.HasValue)
        {
            periodInfo = $"Từ {startDate.Value:dd/MM/yyyy}";
        } else if (endDate.HasValue)
        {
            periodInfo = $"Đến {endDate.Value:dd/MM/yyyy}";
        }
        worksheet.Cell("A2").Value = $"Kỳ báo cáo: {periodInfo} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:K2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers =
        {
            "STT",
            "Ngày GD",
            "Mã Chứng Từ",
            "Loại GD",
            "Sản Phẩm / Phiên Bản",
            "Đối Tác",
            "Nhập",
            "Xuất",
            "Đơn Giá",
            "Thành Tiền",
            "Tồn Sau GD"
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
        worksheet.Column(1).Width = 8;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 40;
        worksheet.Column(6).Width = 25;
        worksheet.Column(7).Width = 12;
        worksheet.Column(8).Width = 12;
        worksheet.Column(9).Width = 15;
        worksheet.Column(10).Width = 15;
        worksheet.Column(11).Width = 12;
        int rowIndex = 5;
        int stt = 1;
        foreach (var item in entries)
        {
            worksheet.Row(rowIndex).Height = 24;
            worksheet.Cell(rowIndex, 1).Value = stt++;
            worksheet.Cell(rowIndex, 2).Value = item.TransactionDate.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(rowIndex, 3).Value = item.DocumentCode ?? string.Empty;
            string typeName = item.ImportQty > 0 ? "Nhập kho" : (item.ExportQty > 0 ? "Xuất kho" : "Điều chỉnh");
            worksheet.Cell(rowIndex, 4).Value = typeName;
            string productName = item.ProductVariant?.Product?.Name ?? string.Empty;
            string variantName = item.ProductVariant?.VariantName ?? string.Empty;
            string colorName = item.ProductVariantColor?.ColorName ?? string.Empty;
            string fullProductName = productName;
            if (!string.IsNullOrEmpty(variantName))
                fullProductName += $" - {variantName}";
            if (!string.IsNullOrEmpty(colorName))
                fullProductName += $" ({colorName})";
            worksheet.Cell(rowIndex, 5).Value = fullProductName;
            worksheet.Cell(rowIndex, 6).Value = string.IsNullOrEmpty(item.PartnerName) ? "—" : item.PartnerName;
            worksheet.Cell(rowIndex, 7).Value = item.ImportQty;
            worksheet.Cell(rowIndex, 8).Value = item.ExportQty;
            worksheet.Cell(rowIndex, 9).Value = item.UnitPrice;
            worksheet.Cell(rowIndex, 9).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(rowIndex, 10).Value = item.TotalAmount;
            worksheet.Cell(rowIndex, 10).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(rowIndex, 11).Value = item.StockAfter;
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            worksheet.Cell(rowIndex, 8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            worksheet.Cell(rowIndex, 9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            worksheet.Cell(rowIndex, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            worksheet.Cell(rowIndex, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            for (int i = 1; i <= 11; i++)
            {
                var cell = worksheet.Cell(rowIndex, i);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }
            rowIndex++;
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
