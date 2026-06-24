using Application.Common.Models;
using ClosedXML.Excel;
using MediatR;
using System.IO;

namespace Application.Features.Suppliers.Queries.GetImportSupplierTemplate;

public class GetImportSupplierTemplateQueryHandler : IRequestHandler<GetImportSupplierTemplateQuery, Result<FileStreamResult>>
{
    public Task<Result<FileStreamResult>> Handle(GetImportSupplierTemplateQuery request, CancellationToken cancellationToken)
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

        // Data Validation for Partner Type
        var typeValidation = worksheet.Range("A5:A1004").CreateDataValidation();
        typeValidation.AllowedValues = XLAllowedValues.List;
        typeValidation.List(Domain.Constants.PartnerType.ExcelValidationList);
        typeValidation.ErrorStyle = XLErrorStyle.Stop;
        typeValidation.ErrorTitle = "Lỗi nhập liệu";
        typeValidation.ErrorMessage = "Vui lòng chọn loại đối tác từ danh sách thả xuống.";

        // Format Text for Tax Id and Phone
        worksheet.Range("C5:C1004").Style.NumberFormat.Format = "@";
        worksheet.Range("E5:E1004").Style.NumberFormat.Format = "@";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_nhap_doi_tac.xlsx");
        return Task.FromResult(Result<FileStreamResult>.Success(fileResult));
    }
}
