using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryOnHand;
using ClosedXML.Excel;
using MediatR;

namespace Application.Features.InventoryReports.Queries.ExportInventoryReport
{
    public class ExportInventoryReportQueryHandler(IInventoryOnHandReadRepository readRepository) : IRequestHandler<ExportInventoryReportQuery, Result<FileStreamResult>>
    {
        public async Task<Result<FileStreamResult>> Handle(
            ExportInventoryReportQuery request,
            CancellationToken cancellationToken)
        {
            var targetMonth = request.Month ?? DateTimeOffset.UtcNow.Month;
            var targetYear = request.Year ?? DateTimeOffset.UtcNow.Year;

            var items = await readRepository.GetInventoryReportSummaryRowsAsync(
                request.SearchTerm,
                request.Month,
                request.Year,
                cancellationToken)
                .ConfigureAwait(false);

            if (items == null || !items.Any())
            {
                return Result<FileStreamResult>.Failure($"Không có dữ liệu xuất nhập tồn trong tháng {targetMonth}/{targetYear}.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo xuất nhập tồn");

            worksheet.Row(1).Height = 40;
            worksheet.Row(2).Height = 20;
            worksheet.Row(3).Height = 15;
            worksheet.Row(4).Height = 30;

            worksheet.Cell("A1").Value = "BÁO CÁO XUẤT NHẬP TỒN KHO";
            var titleRange = worksheet.Range("A1:G1");
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
            titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            string periodInfo = $"Tháng {targetMonth}/{targetYear}";

            worksheet.Cell("A2").Value = $"Kỳ báo cáo: {periodInfo} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
            var subtitleRange = worksheet.Range("A2:G2");
            subtitleRange.Merge();
            subtitleRange.Style.Font.Italic = true;
            subtitleRange.Style.Font.FontSize = 10;
            subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            string[] headers = { "STT", "Tên Sản Phẩm", "Phiên Bản / Màu Sắc", "Tồn Đầu Kỳ", "Nhập Trong Kỳ", "Xuất Trong Kỳ", "Tồn Cuối Kỳ" };
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
            worksheet.Column(3).Width = 35;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 15;

            int rowIndex = 5;
            int stt = 1;

            foreach (var item in items)
            {
                worksheet.Row(rowIndex).Height = 24;
                worksheet.Cell(rowIndex, 1).Value = stt++;
                worksheet.Cell(rowIndex, 2).Value = item.ProductName ?? string.Empty;
                
                string variantStr = item.VariantName ?? string.Empty;
                if (!string.IsNullOrEmpty(item.ColorName))
                {
                    variantStr += $" - {item.ColorName}";
                }
                worksheet.Cell(rowIndex, 3).Value = variantStr;
                
                worksheet.Cell(rowIndex, 4).Value = item.BeginningQty;
                worksheet.Cell(rowIndex, 5).Value = item.ImportedQty;
                worksheet.Cell(rowIndex, 6).Value = item.ExportedQty;
                worksheet.Cell(rowIndex, 7).Value = item.StockQty;

                worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(rowIndex, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Cell(rowIndex, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(rowIndex, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(rowIndex, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(rowIndex, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                for (int i = 1; i <= 7; i++)
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
            var content = stream.ToArray();

            var fileResult = new FileStreamResult(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Bao_cao_xuat_nhap_ton_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            return Result<FileStreamResult>.Success(fileResult);
        }
    }
}
