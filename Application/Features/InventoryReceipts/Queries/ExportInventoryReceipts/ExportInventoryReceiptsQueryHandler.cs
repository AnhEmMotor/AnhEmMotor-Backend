using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Repositories.InventoryReceipt;
using ClosedXML.Excel;
using MediatR;
using System.IO;

namespace Application.Features.InventoryReceipts.Queries.ExportInventoryReceipts;

public class ExportInventoryReceiptsQueryHandler(
    IInventoryReceiptReadRepository readRepository) : IRequestHandler<ExportInventoryReceiptsQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportInventoryReceiptsQuery request, CancellationToken cancellationToken)
    {
        request.SieveModel.PageSize = 100000;
        request.SieveModel.Page = 1;

        var pagedResult = await readRepository.GetPagedAsync<InventoryReceiptListResponse>(
            request.SieveModel, 
            Domain.Constants.DataFetchMode.ActiveOnly,
            null,
            cancellationToken).ConfigureAwait(false);
        var receipts = pagedResult.Items ?? [];
        var receiptIds = receipts.Where(r => r.Id != null).Select(r => r.Id!.Value).ToList();
        var items = new List<Domain.Entities.InventoryReceiptInfo>();
        if (receiptIds.Any())
        {
            items = await readRepository.GetInfosByInventoryReceiptIdsAsync(receiptIds, cancellationToken).ConfigureAwait(false);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Phiếu nhập");

        worksheet.Row(1).Height = 40;
        worksheet.Row(2).Height = 20;
        worksheet.Row(3).Height = 15;
        worksheet.Row(4).Height = 30;

        worksheet.Cell("A1").Value = "DANH SÁCH PHIẾU NHẬP KHO";
        var titleRange = worksheet.Range("A1:J1");
        titleRange.Merge();
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 16;
        titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
        titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        string[] headers = { "Mã phiếu", "Ghi chú", "ID Yêu cầu mua hàng", "Tên sản phẩm", "Tên biến thể sản phẩm", "Tên biến thể màu sắc của sản phẩm", "Số lượng", "Số khung", "Số máy", "Trạng thái" };
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

        int rowIndex = 5;
        foreach (var r in receipts)
        {
            var statusName = r.StatusId switch
            {
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Draft => "Phiếu tạm",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent => "Đã gửi",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve => "Đã duyệt",
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Reject => "Đã từ chối",
                _ => r.StatusId ?? string.Empty
            };

            var rItems = items.Where(x => x.InventoryReceiptId == r.Id).ToList();
            if (rItems.Count == 0)
            {
                worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                worksheet.Cell(rowIndex, 10).Value = statusName;
                rowIndex++;
            }
            else
            {
                foreach (var item in rItems)
                {
                    var prId = item.PurchaseRequestItem?.PurchaseRequestId.ToString() ?? string.Empty;
                    var productName = item.PurchaseRequestItem?.ProductVariant?.Product?.Name ?? string.Empty;
                    var variantName = item.PurchaseRequestItem?.ProductVariant?.VariantName ?? string.Empty;
                    var colorName = item.PurchaseRequestItem?.ProductVariantColor?.ColorName ?? string.Empty;

                    if (item.Vehicles != null && item.Vehicles.Any())
                    {
                        foreach (var v in item.Vehicles)
                        {
                            worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                            worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                            worksheet.Cell(rowIndex, 3).Value = prId;
                            worksheet.Cell(rowIndex, 4).Value = productName;
                            worksheet.Cell(rowIndex, 5).Value = variantName;
                            worksheet.Cell(rowIndex, 6).Value = colorName;
                            worksheet.Cell(rowIndex, 7).Value = item.Count ?? 0;
                            worksheet.Cell(rowIndex, 8).Value = v.VinNumber ?? string.Empty;
                            worksheet.Cell(rowIndex, 9).Value = v.EngineNumber ?? string.Empty;
                            worksheet.Cell(rowIndex, 10).Value = statusName;
                            rowIndex++;
                        }
                    }
                    else
                    {
                        worksheet.Cell(rowIndex, 1).Value = r.Id.ToString();
                        worksheet.Cell(rowIndex, 2).Value = r.Notes ?? string.Empty;
                        worksheet.Cell(rowIndex, 3).Value = prId;
                        worksheet.Cell(rowIndex, 4).Value = productName;
                        worksheet.Cell(rowIndex, 5).Value = variantName;
                        worksheet.Cell(rowIndex, 6).Value = colorName;
                        worksheet.Cell(rowIndex, 7).Value = item.Count ?? 0;
                        worksheet.Cell(rowIndex, 8).Value = string.Empty;
                        worksheet.Cell(rowIndex, 9).Value = string.Empty;
                        worksheet.Cell(rowIndex, 10).Value = statusName;
                        rowIndex++;
                    }
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
