using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using ClosedXML.Excel;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.InventoryReceipts.Queries.GetImportInventoryReceiptTemplate;

public class GetImportInventoryReceiptTemplateQueryHandler(IPurchaseRequestReadRepository purchaseRequestReadRepository) : IRequestHandler<GetImportInventoryReceiptTemplateQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(
        GetImportInventoryReceiptTemplateQuery request,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Thêm Phiếu Nhập");
        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;
        worksheet.Cell("A1").Value = "MẪU NHẬP PHIẾU NHẬP KHO (THEO MẶT HÀNG)";
        var titleRange = worksheet.Range("A1:H1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các ô tô màu vàng là các ô cần nhập thông tin.";
        var subtitleRange = worksheet.Range("A2:H2");
        subtitleRange.Merge();
        subtitleRange.Style.Font.Italic = true;
        subtitleRange.Style.Font.FontSize = 10;
        subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
        subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        worksheet.Cell("A3").Value = "Ghi chú cho toàn bộ phiếu:";
        worksheet.Cell("A3").Style.Font.Bold = true;
        worksheet.Cell("A3").Style.Font.Italic = true;
        worksheet.Cell("A3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        var noteRange = worksheet.Range("B3:H3");
        noteRange.Merge();
        noteRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        string[] headers =
        {
            "Mã yêu cầu mua hàng",
            "Tên sản phẩm",
            "Tên biến thể sản phẩm",
            "Tên biến thể màu sắc của sản phẩm",
            "Số lượng còn lại",
            "Số lượng nhập",
            "Số khung",
            "Số máy"
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
        worksheet.Column(1).Width = 25;
        worksheet.Column(2).Width = 40;
        worksheet.Column(3).Width = 25;
        worksheet.Column(4).Width = 25;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 20;
        var items = await purchaseRequestReadRepository.GetItemsByPurchaseRequestIdsAsync(
            new[] { request.PurchaseRequestId },
            cancellationToken);
        int currentRow = 5;
        foreach (var item in items)
        {
            int receivedCount = item.InventoryReceiptInfos?.Sum(iri => iri.Count ?? 0) ?? 0;
            int remainingCount = item.Quantity - receivedCount;
            if (remainingCount <= 0)
                continue;
            string managementType = item.ProductVariant?.Product?.ProductCategory?.ManagementType ?? "sku";
            if (managementType == "vin_number")
            {
                for (int j = 0; j < remainingCount; j++)
                {
                    FillRowData(worksheet, currentRow, item, remainingCount, j == 0);
                    worksheet.Cell(currentRow, 7).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    worksheet.Cell(currentRow, 8).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    currentRow++;
                }
            } else
            {
                FillRowData(worksheet, currentRow, item, remainingCount, true);
                worksheet.Cell(currentRow, 6).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                currentRow++;
            }
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result<byte[]>.Success(stream.ToArray());
    }

    private void FillRowData(
        IXLWorksheet worksheet,
        int row,
        PurchaseRequestItem item,
        int remainingCount,
        bool showRemainingCount)
    {
        worksheet.Cell(row, 1).Value = item.PurchaseRequestId;
        worksheet.Cell(row, 2).Value = item.ProductVariant?.Product?.Name ?? string.Empty;
        worksheet.Cell(row, 3).Value = item.ProductVariant?.VariantName ?? string.Empty;
        worksheet.Cell(row, 4).Value = item.ProductVariantColor?.ColorName ?? string.Empty;
        if (showRemainingCount)
        {
            worksheet.Cell(row, 5).Value = remainingCount;
        } else
        {
            worksheet.Cell(row, 5).Value = string.Empty;
        }
    }
}
