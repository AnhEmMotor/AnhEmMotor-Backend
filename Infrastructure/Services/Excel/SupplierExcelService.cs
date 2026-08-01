using Application.Interfaces.Services.Excel;
using ClosedXML.Excel;
using Domain.Entities;
using static Domain.Constants.PartnerType;

namespace Infrastructure.Services.Excel;

public class SupplierExcelService : ISupplierExcelService
{
    public byte[] ExportSuppliers(IReadOnlyList<Supplier> suppliers)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Nhà cung cấp");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "DANH SÁCH NHÀ CUNG CẤP";
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
        string[] headers = { "Loại đối tác", "Tên đối tác", "Điện thoại", "Email", "Mã số thuế", "Địa chỉ", "Ghi chú" };
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
        worksheet.Column(2).Width = 35;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 50;
        worksheet.Column(7).Width = 40;
        int rowIndex = 5;
        foreach (var supplier in suppliers)
        {
            worksheet.Row(rowIndex).Height = 24;
            worksheet.Cell(rowIndex, 1).Value = GetName(supplier.PartnerTypeId);
            worksheet.Cell(rowIndex, 2).Value = supplier.Name ?? string.Empty;
            worksheet.Cell(rowIndex, 3).Value = supplier.Phone ?? string.Empty;
            worksheet.Cell(rowIndex, 4).Value = supplier.Email ?? string.Empty;
            worksheet.Cell(rowIndex, 5).Value = supplier.TaxIdentificationNumber ?? string.Empty;
            worksheet.Cell(rowIndex, 6).Value = supplier.Address ?? string.Empty;
            worksheet.Cell(rowIndex, 7).Value = supplier.Notes ?? string.Empty;
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 3).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(rowIndex, 4).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 5).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(rowIndex, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell(rowIndex, 7).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            for (int i = 1; i <= 7; i++)
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
        var worksheet = workbook.Worksheets.Add("Thêm đối tác");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "MẪU NHẬP DANH SÁCH ĐỐI TÁC";
        var titleRange = worksheet.Range("A1:G1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = "Lưu ý: Không thay đổi cấu trúc các cột trong file này";
        var subtitleRange = worksheet.Range("A2:G2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers = { "Loại đối tác", "Tên đối tác", "Điện thoại", "Email", "Mã số thuế", "Địa chỉ", "Ghi chú" };
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
        worksheet.Column(2).Width = 35;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 50;
        worksheet.Column(7).Width = 40;
        var typeValidation = worksheet.Range("A5:A1004").CreateDataValidation();
        typeValidation.AllowedValues = XLAllowedValues.List;
        typeValidation.List(ExcelValidationList);
        typeValidation.ErrorStyle = XLErrorStyle.Stop;
        typeValidation.ErrorTitle = "Lỗi nhập liệu";
        typeValidation.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";
        worksheet.Range("C5:C1004").Style.NumberFormat.Format = "@";
        worksheet.Range("E5:E1004").Style.NumberFormat.Format = "@";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public IReadOnlyList<SupplierImportRow>? ParseImportRows(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return null;
        }
        var rows = new List<SupplierImportRow>();
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return rows;
        }
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var partnerTypeIdRaw = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            var name = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var phone = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var email = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            var taxId = row.Cell(5).GetString()?.Trim() ?? string.Empty;
            var address = row.Cell(6).GetString()?.Trim() ?? string.Empty;
            var notes = row.Cell(7).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(phone) &&
                string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(address))
            {
                continue;
            }
            rows.Add(new SupplierImportRow(partnerTypeIdRaw, name, phone, email, taxId, address, notes));
        }
        return rows;
    }

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(IReadOnlyList<SupplierImportFailedRow> failedRows)
    {
        using var wb1 = new XLWorkbook();
        var ws1 = wb1.Worksheets.Add("Lỗi nhập");
        ws1.Row(1).Height = 40;
        ws1.Row(2).Height = 20;
        ws1.Row(3).Height = 15;
        ws1.Row(4).Height = 30;
        ws1.Cell("A1").Value = "DANH SÁCH LỖI NHẬP ĐỐI TÁC";
        var titleRange1 = ws1.Range("A1:G1");
        titleRange1.Merge();
        titleRange1.Style.Font.Bold = true;
        titleRange1.Style.Font.FontSize = 16;
        titleRange1.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws1.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange1 = ws1.Range("A2:G2");
        subtitleRange1.Merge();
        subtitleRange1.Style.Font.Italic = true;
        subtitleRange1.Style.Font.FontSize = 10;
        subtitleRange1.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange1.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers1 =
        {
            "Loại đối tác",
            "Tên đối tác",
            "Điện thoại",
            "Email",
            "Mã số thuế",
            "Địa chỉ",
            "Ghi chú"
        };
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
        ws1.Column(1).Width = 15;
        ws1.Column(2).Width = 25;
        ws1.Column(3).Width = 15;
        ws1.Column(4).Width = 25;
        ws1.Column(5).Width = 15;
        ws1.Column(6).Width = 40;
        ws1.Column(7).Width = 30;
        var typeValidation1 = ws1.Range($"A5:A{Math.Max(5, failedRows.Count + 4)}").CreateDataValidation();
        typeValidation1.AllowedValues = XLAllowedValues.List;
        typeValidation1.List(ExcelValidationList);
        typeValidation1.ErrorStyle = XLErrorStyle.Stop;
        typeValidation1.ErrorTitle = "Lỗi nhập liệu";
        typeValidation1.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws1.Row(i + 5).Height = 24;
            ws1.Cell(i + 5, 1).Value = failedRows[i].PartnerTypeId;
            ws1.Cell(i + 5, 2).Value = failedRows[i].Name;
            ws1.Cell(i + 5, 3).Value = failedRows[i].Phone;
            ws1.Cell(i + 5, 4).Value = failedRows[i].Email;
            ws1.Cell(i + 5, 5).Value = failedRows[i].TaxIdentificationNumber;
            ws1.Cell(i + 5, 6).Value = failedRows[i].Address;
            ws1.Cell(i + 5, 7).Value = failedRows[i].Notes;
            for (int col = 1; col <= 7; col++)
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
        ws2.Cell("A1").Value = "DANH SÁCH LỖI NHẬP ĐỐI TÁC (KÈM LÝ DO)";
        var titleRange2 = ws2.Range("A1:H1");
        titleRange2.Merge();
        titleRange2.Style.Font.Bold = true;
        titleRange2.Style.Font.FontSize = 16;
        titleRange2.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws2.Cell("A2").Value = $"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange2 = ws2.Range("A2:H2");
        subtitleRange2.Merge();
        subtitleRange2.Style.Font.Italic = true;
        subtitleRange2.Style.Font.FontSize = 10;
        subtitleRange2.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange2.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        string[] headers2 =
        {
            "Loại đối tác",
            "Tên đối tác",
            "Điện thoại",
            "Email",
            "Mã số thuế",
            "Địa chỉ",
            "Ghi chú",
            "Lý Do Lỗi"
        };
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
        ws2.Column(1).Width = 15;
        ws2.Column(2).Width = 25;
        ws2.Column(3).Width = 15;
        ws2.Column(4).Width = 25;
        ws2.Column(5).Width = 15;
        ws2.Column(6).Width = 40;
        ws2.Column(7).Width = 30;
        ws2.Column(8).Width = 40;
        var typeValidation2 = ws2.Range($"A5:A{Math.Max(5, failedRows.Count + 4)}").CreateDataValidation();
        typeValidation2.AllowedValues = XLAllowedValues.List;
        typeValidation2.List(ExcelValidationList);
        typeValidation2.ErrorStyle = XLErrorStyle.Stop;
        typeValidation2.ErrorTitle = "Lỗi nhập liệu";
        typeValidation2.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";
        for (int i = 0; i < failedRows.Count; i++)
        {
            ws2.Row(i + 5).Height = 24;
            ws2.Cell(i + 5, 1).Value = failedRows[i].PartnerTypeId;
            ws2.Cell(i + 5, 2).Value = failedRows[i].Name;
            ws2.Cell(i + 5, 3).Value = failedRows[i].Phone;
            ws2.Cell(i + 5, 4).Value = failedRows[i].Email;
            ws2.Cell(i + 5, 5).Value = failedRows[i].TaxIdentificationNumber;
            ws2.Cell(i + 5, 6).Value = failedRows[i].Address;
            ws2.Cell(i + 5, 7).Value = failedRows[i].Notes;
            ws2.Cell(i + 5, 8).Value = failedRows[i].Reason;
            for (int col = 1; col <= 8; col++)
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
