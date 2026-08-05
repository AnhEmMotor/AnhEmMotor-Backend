using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Excel;

public class BrandExcelService : IBrandExcelService
{
    public byte[] ExportBrands(IReadOnlyList<Brand> brands)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thương hiệu");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "DANH SÁCH THƯƠNG HIỆU SẢN PHẨM";
        var titleRange = worksheet.Range("A1:E1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:E2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers = { "STT", "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả" };
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
        worksheet.Column(2).Width = 35;
        worksheet.Column(3).Width = 25;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 45;
        int rowIndex = 5;
        int stt = 1;
        foreach (var brand in brands)
        {
            worksheet.Row(rowIndex).Height = 24;
            worksheet.Cell(rowIndex, 1).Value = stt++;
            if (!string.IsNullOrWhiteSpace(brand.LogoUrl))
            {
                var cellUrl = worksheet.Cell(rowIndex, 2);
                cellUrl.Value = brand.LogoUrl;
                if (brand.LogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                    brand.LogoUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    cellUrl.SetHyperlink(new XLHyperlink(brand.LogoUrl));
                    cellUrl.Style.Font.FontColor = XLColor.Blue;
                    cellUrl.Style.Font.Underline = XLFontUnderlineValues.Single;
                }
            } else
            {
                worksheet.Cell(rowIndex, 2).Value = "Chưa cấu hình";
            }
            worksheet.Cell(rowIndex, 3).Value = brand.Name;
            worksheet.Cell(rowIndex, 4).Value = brand.Origin ?? string.Empty;
            worksheet.Cell(rowIndex, 5).Value = brand.Description ?? string.Empty;
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            for (int i = 1; i <= 5; i++)
            {
                var cell = worksheet.Cell(rowIndex, i);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }
            rowIndex++;
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] BuildImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thương hiệu");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "MẪU NHẬP THƯƠNG HIỆU SẢN PHẨM";
        var titleRange = worksheet.Range("A1:E1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = $"Ngày tạo mẫu: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:E2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers = { "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả" };
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
        worksheet.Column(1).Width = 35;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 45;
        int rowIndex = 5;
        worksheet.Row(rowIndex).Height = 24;
        worksheet.Cell(rowIndex, 1).Value = "https://example.com/logo.png";
        worksheet.Cell(rowIndex, 2).Value = "Tên mẫu (Vui lòng xóa dòng này)";
        worksheet.Cell(rowIndex, 3).Value = "Việt Nam";
        worksheet.Cell(rowIndex, 4).Value = "Mô tả mẫu";
        worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        worksheet.Cell(rowIndex, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        worksheet.Cell(rowIndex, 2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        worksheet.Cell(rowIndex, 3).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        worksheet.Cell(rowIndex, 4).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        for (int i = 1; i <= 4; i++)
        {
            var cell = worksheet.Cell(rowIndex, i);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            cell.Style.Font.FontSize = 11;
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public IReadOnlyList<BrandImportRow> ParseImportRows(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return [];
        }
        var rows = new List<BrandImportRow>();
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return rows;
        }
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var logoUrl = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            if (logoUrl == "Chưa cấu hình")
                logoUrl = string.Empty;
            var name = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var origin = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var description = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(origin) &&
                string.IsNullOrWhiteSpace(description))
            {
                continue;
            }
            rows.Add(new BrandImportRow(logoUrl, name, origin, description));
        }
        return rows;
    }

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(
        IReadOnlyList<BrandImportFailedRow> failedRows)
    {
        using var wb1 = new XLWorkbook();
        var ws1 = wb1.Worksheets.Add("Lỗi nhập");
        ws1.Row(1).Height = 40;
        ws1.Row(2).Height = 20;
        ws1.Row(3).Height = 15;
        ws1.Row(4).Height = 30;
        ws1.Cell("A1").Value = "DANH SÁCH LỖI NHẬP THƯƠNG HIỆU";
        var titleRange1 = ws1.Range("A1:D1");
        titleRange1.Merge();
        titleRange1.Style.Font.Bold = true;
        titleRange1.Style.Font.FontSize = 16;
        titleRange1.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws1.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange1 = ws1.Range("A2:D2");
        subtitleRange1.Merge();
        subtitleRange1.Style.Font.Italic = true;
        subtitleRange1.Style.Font.FontSize = 10;
        subtitleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers1 = { "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả" };
        for (int i = 0; i < headers1.Length; i++)
        {
            var cell = ws1.Cell(4, i + 1);
            cell.Value = headers1[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        ws1.Column(1).Width = 35;
        ws1.Column(2).Width = 25;
        ws1.Column(3).Width = 15;
        ws1.Column(4).Width = 45;
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws1.Row(i + 5).Height = 24;
            ws1.Cell(i + 5, 1).Value = failedRows[i].LogoUrl;
            ws1.Cell(i + 5, 2).Value = failedRows[i].Name;
            ws1.Cell(i + 5, 3).Value = failedRows[i].Origin;
            ws1.Cell(i + 5, 4).Value = failedRows[i].Description;
            ws1.Cell(i + 5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws1.Cell(i + 5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws1.Cell(i + 5, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws1.Cell(i + 5, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            for (int col = 1; col <= 4; col++)
            {
                var cell = ws1.Cell(i + 5, col);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }
        }
        using var stream1 = new MemoryStream();
        wb1.SaveAs(stream1);
        using var wb2 = new XLWorkbook();
        var ws2 = wb2.Worksheets.Add("Lỗi nhập");
        ws2.Row(1).Height = 40;
        ws2.Row(2).Height = 20;
        ws2.Row(3).Height = 15;
        ws2.Row(4).Height = 30;
        ws2.Cell("A1").Value = "DANH SÁCH LỖI NHẬP THƯƠNG HIỆU (KÈM LÝ DO)";
        var titleRange2 = ws2.Range("A1:E1");
        titleRange2.Merge();
        titleRange2.Style.Font.Bold = true;
        titleRange2.Style.Font.FontSize = 16;
        titleRange2.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws2.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange2 = ws2.Range("A2:E2");
        subtitleRange2.Merge();
        subtitleRange2.Style.Font.Italic = true;
        subtitleRange2.Style.Font.FontSize = 10;
        subtitleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers2 = { "Đường Dẫn URL Logo", "Tên Thương Hiệu", "Xuất Xứ", "Mô Tả", "Lý Do Lỗi" };
        for (int i = 0; i < headers2.Length; i++)
        {
            var cell = ws2.Cell(4, i + 1);
            cell.Value = headers2[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        ws2.Column(1).Width = 35;
        ws2.Column(2).Width = 25;
        ws2.Column(3).Width = 15;
        ws2.Column(4).Width = 45;
        ws2.Column(5).Width = 40;
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws2.Row(i + 5).Height = 24;
            ws2.Cell(i + 5, 1).Value = failedRows[i].LogoUrl;
            ws2.Cell(i + 5, 2).Value = failedRows[i].Name;
            ws2.Cell(i + 5, 3).Value = failedRows[i].Origin;
            ws2.Cell(i + 5, 4).Value = failedRows[i].Description;
            ws2.Cell(i + 5, 5).Value = failedRows[i].Reason;
            ws2.Cell(i + 5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws2.Cell(i + 5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws2.Cell(i + 5, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws2.Cell(i + 5, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws2.Cell(i + 5, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            for (int col = 1; col <= 5; col++)
            {
                var cell = ws2.Cell(i + 5, col);
                cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }
        }
        using var stream2 = new MemoryStream();
        wb2.SaveAs(stream2);
        return (stream1.ToArray(), stream2.ToArray());
    }
}
