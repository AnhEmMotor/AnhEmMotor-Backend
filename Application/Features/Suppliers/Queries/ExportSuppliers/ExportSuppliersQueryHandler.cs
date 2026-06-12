using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using ClosedXML.Excel;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.ExportSuppliers;

public sealed class ExportSuppliersQueryHandler(ISupplierReadRepository repository) : IRequestHandler<ExportSuppliersQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await repository.GetFilteredListAsync(
            request.SieveModel ?? new SieveModel(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
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
        string[] headers = { "STT", "Tên Đối Tác", "Loại Đối Tác", "Điện Thoại", "Email", "Mã Số Thuế", "Địa Chỉ" };
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
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 18;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 25;
        worksheet.Column(6).Width = 18;
        worksheet.Column(7).Width = 35;
        int rowIndex = 5;
        int stt = 1;
        foreach (var supplier in suppliers)
        {
            worksheet.Row(rowIndex).Height = 24;
            worksheet.Cell(rowIndex, 1).Value = stt++;
            worksheet.Cell(rowIndex, 2).Value = supplier.Name ?? string.Empty;
            worksheet.Cell(rowIndex, 3).Value = PartnerType.GetName(supplier.PartnerTypeId);
            worksheet.Cell(rowIndex, 4).Value = supplier.Phone ?? string.Empty;
            worksheet.Cell(rowIndex, 5).Value = supplier.Email ?? string.Empty;
            worksheet.Cell(rowIndex, 6).Value = supplier.TaxIdentificationNumber ?? string.Empty;
            worksheet.Cell(rowIndex, 7).Value = supplier.Address ?? string.Empty;
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
            worksheet.Cell(rowIndex, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
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
        var content = stream.ToArray();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_nha_cung_cap.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }
}