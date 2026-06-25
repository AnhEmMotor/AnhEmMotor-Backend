using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.PurchaseRequest;
using ClosedXML.Excel;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.InventoryReceipts.Commands.ImportInventoryReceipts;

public class ImportInventoryReceiptsCommandHandler(
    IInventoryReceiptInsertRepository insertRepository,
    IPurchaseRequestReadRepository purchaseRequestReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IRequestHandler<ImportInventoryReceiptsCommand, Result<ImportInventoryReceiptsResult>>
{
    public async Task<Result<ImportInventoryReceiptsResult>> Handle(
        ImportInventoryReceiptsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Result<ImportInventoryReceiptsResult>.Failure(Error.BadRequest("Không tìm thấy file tải lên."));
        }
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var fileBytes = memoryStream.ToArray();
        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return Result<ImportInventoryReceiptsResult>.Failure(
                Error.BadRequest("Excel file does not contain any worksheet."));
        }
        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 5)
        {
            return Result<ImportInventoryReceiptsResult>.Success(new ImportInventoryReceiptsResult());
        }
        var parsedRows = new List<(string PrIdStr, string ProductName, string VariantName, string ColorName, string RemQtyStr, string QtyStr, string Vin, string Engine, string Note, int RowIndex)>(
            );
        var uniquePrIds = new HashSet<int>();
        var generalNote = worksheet.Cell(3, 2).GetString()?.Trim() ?? string.Empty;
        for (int i = 5; i <= rowCount; i++)
        {
            var row = worksheet.Row(i);
            var prIdStr = row.Cell(1).GetString()?.Trim() ?? string.Empty;
            var productName = row.Cell(2).GetString()?.Trim() ?? string.Empty;
            var variantName = row.Cell(3).GetString()?.Trim() ?? string.Empty;
            var colorName = row.Cell(4).GetString()?.Trim() ?? string.Empty;
            var remQtyStr = row.Cell(5).GetString()?.Trim() ?? string.Empty;
            var qtyStr = row.Cell(6).GetString()?.Trim() ?? string.Empty;
            var vin = row.Cell(7).GetString()?.Trim() ?? string.Empty;
            var engine = row.Cell(8).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(qtyStr) && string.IsNullOrWhiteSpace(vin) && string.IsNullOrWhiteSpace(engine))
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(qtyStr) &&
                (!string.IsNullOrWhiteSpace(vin) || !string.IsNullOrWhiteSpace(engine)))
            {
                qtyStr = "1";
            }
            if (string.IsNullOrWhiteSpace(prIdStr) && string.IsNullOrWhiteSpace(productName))
            {
                continue;
            }
            parsedRows.Add(
                (prIdStr, productName, variantName, colorName, remQtyStr, qtyStr, vin, engine, generalNote, i));
            if (int.TryParse(prIdStr, out var prId) && prId > 0)
            {
                uniquePrIds.Add(prId);
            }
        }
        var prItems = new List<PurchaseRequestItem>();
        if (uniquePrIds.Count > 0)
        {
            prItems = await purchaseRequestReadRepository.GetItemsByPurchaseRequestIdsAsync(
                uniquePrIds,
                cancellationToken)
                .ConfigureAwait(false);
        }
        var tempCodeGroup = new Dictionary<string, List<InventoryReceiptInfo>>();
        var tempCodeNotes = new Dictionary<string, string>();
        var tempCodePrId = new Dictionary<string, int>();
        var failedRowsData = new List<(string PrId, string ProductName, string VariantName, string ColorName, string RemQtyStr, string Qty, string Vin, string Engine, string Note, string Reason)>(
            );
        foreach (var r in parsedRows)
        {
            var rowErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(r.PrIdStr))
                rowErrors.Add("Thiếu ID Yêu cầu mua hàng.");
            if (string.IsNullOrWhiteSpace(r.QtyStr))
                rowErrors.Add("Thiếu số lượng.");
            if (string.IsNullOrWhiteSpace(r.ProductName))
                rowErrors.Add("Thiếu tên sản phẩm.");
            int prId = 0, qty = 0;
            if (!string.IsNullOrWhiteSpace(r.PrIdStr) && !int.TryParse(r.PrIdStr, out prId))
                rowErrors.Add("ID YCMH không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(r.QtyStr) && !int.TryParse(r.QtyStr, out qty))
                rowErrors.Add("Số lượng không hợp lệ.");
            int prItemId = 0;
            if (prId > 0 && rowErrors.Count == 0)
            {
                var matchedItem = prItems.FirstOrDefault(
                    x => x.PurchaseRequestId == prId &&
                        x.ProductVariant?.Product?.Name == r.ProductName &&
                        x.ProductVariant?.VariantName == r.VariantName &&
                        (string.IsNullOrWhiteSpace(r.ColorName)
                            ? x.ProductVariantColor == null
                            : x.ProductVariantColor?.ColorName == r.ColorName));
                if (matchedItem == null)
                {
                    rowErrors.Add("Không tìm thấy chi tiết YCMH khớp với Tên sản phẩm, Biến thể và Màu sắc.");
                } else
                {
                    prItemId = matchedItem.Id;
                }
            }
            if (rowErrors.Count > 0)
            {
                failedRowsData.Add(
                    (r.PrIdStr, r.ProductName, r.VariantName, r.ColorName, r.RemQtyStr, r.QtyStr, r.Vin, r.Engine, r.Note, string.Join(
                        ", ",
                        rowErrors)));
            } else
            {
                var defaultTempCode = "DEFAULT";
                if (!tempCodeGroup.ContainsKey(defaultTempCode))
                {
                    tempCodeGroup[defaultTempCode] = new List<InventoryReceiptInfo>();
                    tempCodeNotes[defaultTempCode] = r.Note;
                    tempCodePrId[defaultTempCode] = prId;
                }
                var list = tempCodeGroup[defaultTempCode];
                var existingInfo = list.FirstOrDefault(x => x.PurchaseRequestItemId == prItemId);
                if (existingInfo == null)
                {
                    existingInfo = new InventoryReceiptInfo
                    {
                        PurchaseRequestItemId = prItemId,
                        Count = qty,
                        Vehicles = new List<Vehicle>()
                    };
                    list.Add(existingInfo);
                } else
                {
                    existingInfo.Count += qty;
                }
                if (!string.IsNullOrWhiteSpace(r.Vin) || !string.IsNullOrWhiteSpace(r.Engine))
                {
                    existingInfo.Vehicles
                        .Add(new Vehicle { VinNumber = r.Vin, EngineNumber = r.Engine, ImportPrice = 0 });
                }
            }
        }
        var successCount = 0;
        foreach (var group in tempCodeGroup)
        {
            var receipt = new InventoryReceipt
            {
                StatusId = "draft",
                Notes = tempCodeNotes[group.Key],
                PurchaseRequestId = tempCodePrId[group.Key] > 0 ? tempCodePrId[group.Key] : null,
                InventoryReceiptDate = DateTimeOffset.Now,
                InventoryReceiptInfos = group.Value
            };
            insertRepository.Add(receipt);
            successCount++;
        }
        if (successCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var result = new ImportInventoryReceiptsResult
        {
            SuccessCount = successCount,
            FailedCount = failedRowsData.Count
        };
        if (failedRowsData.Count > 0)
        {
            var customUploadPath = configuration["LocalFileStorage:UploadPath"];
            var wwwrootPath = !string.IsNullOrWhiteSpace(customUploadPath)
                ? (Path.IsPathRooted(customUploadPath)
                    ? customUploadPath
                    : Path.Combine(Directory.GetCurrentDirectory(), customUploadPath))
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var errorsDir = Path.Combine(wwwrootPath, "import-errors");
            if (!Directory.Exists(errorsDir))
                Directory.CreateDirectory(errorsDir);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var file1Name = $"ImportErrors_InventoryReceipt_{timestamp}.xlsx";
            var file2Name = $"ImportErrors_InventoryReceipt_WithReason_{timestamp}.xlsx";
            string[] headers1 =
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
            Action<IXLWorksheet> addTitleRows = (ws) =>
            {
                ws.Row(1).Height = 40;
                ws.Row(2).Height = 20;
                ws.Row(3).Height = 15;
                ws.Row(4).Height = 30;
                ws.Cell("A1").Value = "MẪU NHẬP PHIẾU NHẬP KHO (THEO MẶT HÀNG)";
                var titleRange = ws.Range("A1:H1");
                titleRange.Merge();
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 16;
                titleRange.Style.Font.FontColor = XLColor.FromHtml("#1A365D");
                titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                titleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("A2").Value = "Lưu ý: Mỗi dòng là 1 mặt hàng. Các ô tô màu vàng là các ô cần nhập thông tin.";
                var subtitleRange = ws.Range("A2:H2");
                subtitleRange.Merge();
                subtitleRange.Style.Font.Italic = true;
                subtitleRange.Style.Font.FontSize = 10;
                subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#EF5350");
                subtitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                subtitleRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell("A3").Value = "Ghi chú cho toàn bộ phiếu:";
                ws.Cell("A3").Style.Font.Bold = true;
                ws.Cell("A3").Style.Font.Italic = true;
                ws.Cell("A3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell("B3").Value = generalNote;
                var noteRange = ws.Range("B3:H3");
                noteRange.Merge();
                noteRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                for (int i = 0; i < headers1.Length; i++)
                {
                    var cell = ws.Cell(4, i + 1);
                    cell.Value = headers1[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#EF5350"));
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
                ws.Column(1).Width = 25;
                ws.Column(2).Width = 40;
                ws.Column(3).Width = 25;
                ws.Column(4).Width = 25;
                ws.Column(5).Width = 20;
                ws.Column(6).Width = 20;
                ws.Column(7).Width = 20;
                ws.Column(8).Width = 20;
            };
            using (var wb1 = new XLWorkbook())
            {
                var ws1 = wb1.Worksheets.Add("Lỗi nhập");
                addTitleRows(ws1);
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws1.Cell(i + 5, 1).Value = failedRowsData[i].PrId;
                    ws1.Cell(i + 5, 2).Value = failedRowsData[i].ProductName;
                    ws1.Cell(i + 5, 3).Value = failedRowsData[i].VariantName;
                    ws1.Cell(i + 5, 4).Value = failedRowsData[i].ColorName;
                    ws1.Cell(i + 5, 5).Value = failedRowsData[i].RemQtyStr;
                    ws1.Cell(i + 5, 6).Value = failedRowsData[i].Qty;
                    ws1.Cell(i + 5, 7).Value = failedRowsData[i].Vin;
                    ws1.Cell(i + 5, 8).Value = failedRowsData[i].Engine;
                }
                wb1.SaveAs(Path.Combine(errorsDir, file1Name));
            }
            using (var wb2 = new XLWorkbook())
            {
                var ws2 = wb2.Worksheets.Add("Lỗi nhập chi tiết");
                addTitleRows(ws2);
                var cellReason = ws2.Cell(4, 9);
                cellReason.Value = "Lý do lỗi";
                cellReason.Style.Font.Bold = true;
                cellReason.Style.Font.FontColor = XLColor.White;
                cellReason.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E53935"));
                cellReason.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cellReason.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                cellReason.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                ws2.Column(9).Width = 40;
                for (int i = 0; i < failedRowsData.Count; i++)
                {
                    ws2.Cell(i + 5, 1).Value = failedRowsData[i].PrId;
                    ws2.Cell(i + 5, 2).Value = failedRowsData[i].ProductName;
                    ws2.Cell(i + 5, 3).Value = failedRowsData[i].VariantName;
                    ws2.Cell(i + 5, 4).Value = failedRowsData[i].ColorName;
                    ws2.Cell(i + 5, 5).Value = failedRowsData[i].RemQtyStr;
                    ws2.Cell(i + 5, 6).Value = failedRowsData[i].Qty;
                    ws2.Cell(i + 5, 7).Value = failedRowsData[i].Vin;
                    ws2.Cell(i + 5, 8).Value = failedRowsData[i].Engine;
                    ws2.Cell(i + 5, 9).Value = failedRowsData[i].Reason;
                }
                wb2.SaveAs(Path.Combine(errorsDir, file2Name));
            }
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportInventoryReceiptsResult>.Success(result);
    }
}
