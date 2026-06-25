using Application.Common.Models;
using ClosedXML.Excel;
using MediatR;
using System.IO;

namespace Application.Features.PurchaseRequests.Queries.GetImportPurchaseRequestTemplate;

public class GetImportPurchaseRequestTemplateQueryHandler : IRequestHandler<GetImportPurchaseRequestTemplateQuery, Result<byte[]>>
{
    public Task<Result<byte[]>> Handle(GetImportPurchaseRequestTemplateQuery request, CancellationToken cancellationToken)
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

        string[] headers = { "Mã phiếu", "Ghi chú", "Tên sản phẩm", "Tên biến thể sản phẩm", "Tên biến thể màu sắc của sản phẩm (nếu có)", "Số lượng yêu cầu", "Tên nhà cung cấp" };
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
        return Task.FromResult(Result<byte[]>.Success(stream.ToArray()));
    }
}
