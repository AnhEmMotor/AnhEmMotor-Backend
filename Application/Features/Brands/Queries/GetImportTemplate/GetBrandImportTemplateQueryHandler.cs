using Application.Common.Models;
using ClosedXML.Excel;
using MediatR;

namespace Application.Features.Brands.Queries.GetImportTemplate;

public class GetBrandImportTemplateQueryHandler : IRequestHandler<GetBrandImportTemplateQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        GetBrandImportTemplateQuery request,
        CancellationToken cancellationToken)
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
        var content = stream.ToArray();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_nhap_thuong_hieu.xlsx");
        return await Task.FromResult(Result<FileStreamResult>.Success(fileResult));
    }
}
