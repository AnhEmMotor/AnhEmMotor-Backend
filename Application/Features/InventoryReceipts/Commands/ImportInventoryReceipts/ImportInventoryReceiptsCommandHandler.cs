using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services.Excel;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.InventoryReceipts.Commands.ImportInventoryReceipts;

public class ImportInventoryReceiptsCommandHandler(
    IInventoryReceiptInsertRepository insertRepository,
    IPurchaseRequestReadRepository purchaseRequestReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IInventoryReceiptExcelService excelService) : IRequestHandler<ImportInventoryReceiptsCommand, Result<ImportInventoryReceiptsResult>>
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
        var importRows = excelService.ParseImportRows(fileBytes);
        if (importRows == null)
        {
            return Result<ImportInventoryReceiptsResult>.Failure(
                Error.BadRequest("Excel file does not contain any worksheet."));
        }
        if (importRows.Count == 0)
        {
            return Result<ImportInventoryReceiptsResult>.Success(new ImportInventoryReceiptsResult());
        }
        var uniquePrIds = new HashSet<int>();
        foreach (var r in importRows)
        {
            if (int.TryParse(r.PrIdStr, out var prId) && prId > 0)
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
        var failedRowsData = new List<InventoryReceiptImportFailedRow>();
        foreach (var r in importRows)
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
                    new InventoryReceiptImportFailedRow(
                        r.PrIdStr,
                        r.ProductName,
                        r.VariantName,
                        r.ColorName,
                        r.RemQtyStr,
                        r.QtyStr,
                        r.Vin,
                        r.Engine,
                        r.Note,
                        string.Join(", ", rowErrors)));
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
            var (file1Bytes, file2Bytes) = excelService.BuildImportErrorReports(failedRowsData);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file1Name), file1Bytes, cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file2Name), file2Bytes, cancellationToken)
                .ConfigureAwait(false);
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportInventoryReceiptsResult>.Success(result);
    }
}
