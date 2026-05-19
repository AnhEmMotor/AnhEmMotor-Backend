using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using ClosedXML.Excel;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Brands.Queries.ExportBrands;

public sealed class ExportBrandsQueryHandler(IBrandReadRepository repository) : IRequestHandler<ExportBrandsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var brands = await repository.GetFilteredListAsync(
            request.SieveModel ?? new SieveModel(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thương hiệu");

        // Set row heights
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;

        // Title
        worksheet.Cell("A1").Value = "DANH SÁCH THƯƠNG HIỆU SẢN PHẨM";
        var titleRange = worksheet.Range("A1:E1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Date of export
        worksheet.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var subtitleRange = worksheet.Range("A2:E2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Table headers
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

        // Set column widths
        worksheet.Column(1).Width = 8;   // STT
        worksheet.Column(2).Width = 35;  // URL Logo
        worksheet.Column(3).Width = 25;  // Tên Thương Hiệu
        worksheet.Column(4).Width = 15;  // Xuất Xứ
        worksheet.Column(5).Width = 45;  // Mô Tả

        // Data rows
        int rowIndex = 5;
        int stt = 1;

        foreach (var brand in brands)
        {
            // Set nice row height for data
            worksheet.Row(rowIndex).Height = 24;

            worksheet.Cell(rowIndex, 1).Value = stt++;

            // Column 2 (B): URL Logo with clickable hyperlink
            if (!string.IsNullOrWhiteSpace(brand.LogoUrl))
            {
                var cellUrl = worksheet.Cell(rowIndex, 2);
                cellUrl.Value = brand.LogoUrl;
                if (brand.LogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) || brand.LogoUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    cellUrl.SetHyperlink(new XLHyperlink(brand.LogoUrl));
                    cellUrl.Style.Font.FontColor = XLColor.Blue;
                    cellUrl.Style.Font.Underline = XLFontUnderlineValues.Single;
                }
            }
            else
            {
                worksheet.Cell(rowIndex, 2).Value = "Chưa cấu hình";
            }

            worksheet.Cell(rowIndex, 3).Value = brand.Name;
            worksheet.Cell(rowIndex, 4).Value = brand.Origin ?? "";
            worksheet.Cell(rowIndex, 5).Value = brand.Description ?? "";

            // Alignment
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

            // Row styles & borders
            for (int i = 1; i <= 5; i++)
            {
                var cell = worksheet.Cell(rowIndex, i);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                cell.Style.Font.FontSize = 11;
            }

            rowIndex++;
        }

        // Save to stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_thuong_hieu.xlsx");

        return Result<FileStreamResult>.Success(fileResult);
    }
}
