using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Excel;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Application.Features.PurchaseRequests.Commands.ImportPurchaseRequests;

public class ImportPurchaseRequestsCommandHandler(
    IPurchaseRequestInsertRepository insertRepository,
    IProductReadRepository productReadRepository,
    IProductVariantReadRepository variantReadRepository,
    ISupplierReadRepository supplierReadRepository,
    IProductQuotationReadRepository quotationReadRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IPurchaseRequestExcelService excelService) : IRequestHandler<ImportPurchaseRequestsCommand, Result<ImportPurchaseRequestsResult>>
{
    public async Task<Result<ImportPurchaseRequestsResult>> Handle(
        ImportPurchaseRequestsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Result<ImportPurchaseRequestsResult>.Failure(Error.BadRequest("Không tìm thấy file tải lên."));
        }
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var fileBytes = memoryStream.ToArray();
        var importRows = excelService.ParseImportRows(fileBytes);
        if (importRows == null)
        {
            return Result<ImportPurchaseRequestsResult>.Failure(
                Error.BadRequest("Excel file does not contain any worksheet."));
        }
        if (importRows.Count == 0)
        {
            return Result<ImportPurchaseRequestsResult>.Success(new ImportPurchaseRequestsResult());
        }
        var allProducts = await productReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var allVariants = await variantReadRepository.GetAllAsync(
            cancellationToken,
            Domain.Constants.DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var allSuppliers = await supplierReadRepository.GetAllAsync(
            cancellationToken,
            Domain.Constants.DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var allQuotations = await quotationReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var tempCodeGroup = new Dictionary<string, List<PurchaseRequestItem>>();
        var tempCodeNotes = new Dictionary<string, string>();
        var failedRowsData = new List<(string TempCode, string Note, string ProductName, string VariantName, string ColorName, string Qty, string SupplierName, string Reason)>(
            );
        foreach (var importRow in importRows)
        {
            var tempCode = importRow.TempCode;
            var note = importRow.Note;
            var productName = importRow.ProductName;
            var variantName = importRow.VariantName;
            var colorName = importRow.ColorName;
            var qtyStr = importRow.Qty;
            var supplierName = importRow.SupplierName;
            var rowErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(tempCode))
                rowErrors.Add("Thiếu mã phiếu.");
            if (string.IsNullOrWhiteSpace(productName))
                rowErrors.Add("Thiếu tên sản phẩm.");
            if (string.IsNullOrWhiteSpace(variantName))
                rowErrors.Add("Thiếu tên biến thể.");
            if (string.IsNullOrWhiteSpace(qtyStr))
                rowErrors.Add("Thiếu số lượng.");
            if (string.IsNullOrWhiteSpace(supplierName))
                rowErrors.Add("Thiếu tên nhà cung cấp.");
            int qty = 0;
            if (!string.IsNullOrWhiteSpace(qtyStr) && !int.TryParse(qtyStr, out qty))
                rowErrors.Add("Số lượng không hợp lệ.");
            Product? matchedProduct = null;
            ProductVariant? matchedVariant = null;
            ProductVariantColor? matchedColor = null;
            Supplier? matchedSupplier = null;
            ProductQuotation? matchedQuotation = null;
            if (rowErrors.Count == 0)
            {
                matchedProduct = allProducts.FirstOrDefault(
                    p => string.Equals(p.Name, productName, StringComparison.OrdinalIgnoreCase));
                if (matchedProduct == null)
                {
                    rowErrors.Add("Tên sản phẩm không tồn tại.");
                } else
                {
                    matchedVariant = allVariants.FirstOrDefault(
                        v => v.ProductId == matchedProduct.Id &&
                            string.Equals(v.VariantName, variantName, StringComparison.OrdinalIgnoreCase));
                    if (matchedVariant == null)
                    {
                        rowErrors.Add("Tên biến thể không tồn tại hoặc không thuộc sản phẩm này.");
                    } else
                    {
                        if (!string.IsNullOrWhiteSpace(colorName))
                        {
                            matchedColor = matchedVariant.ProductVariantColors?.FirstOrDefault(
                                c => string.Equals(c.ColorName, colorName, StringComparison.OrdinalIgnoreCase));
                            if (matchedColor == null)
                            {
                                rowErrors.Add("Tên màu sắc không tồn tại hoặc không thuộc biến thể này.");
                            }
                        }
                    }
                }
                matchedSupplier = allSuppliers.FirstOrDefault(
                    s => string.Equals(s.Name, supplierName, StringComparison.OrdinalIgnoreCase));
                if (matchedSupplier == null)
                {
                    rowErrors.Add("Tên nhà cung cấp không tồn tại.");
                }
                if (matchedVariant != null &&
                    matchedSupplier != null &&
                    (string.IsNullOrWhiteSpace(colorName) || matchedColor != null))
                {
                    int? colorId = matchedColor?.Id;
                    matchedQuotation = allQuotations.FirstOrDefault(
                        q => q.ProductVariantId == matchedVariant.Id &&
                            q.ProductVariantColorId == colorId &&
                            q.SupplierId == matchedSupplier.Id);
                }
            }
            if (rowErrors.Count == 0 && matchedQuotation == null)
            {
                rowErrors.Add("Không tìm thấy báo giá hợp lệ từ nhà cung cấp cho mặt hàng này.");
            }
            if (rowErrors.Count > 0)
            {
                failedRowsData.Add(
                    (tempCode, note, productName, variantName, colorName, qtyStr, supplierName, string.Join(
                        ", ",
                        rowErrors)));
            } else
            {
                if (!tempCodeGroup.ContainsKey(tempCode))
                {
                    tempCodeGroup[tempCode] = new List<PurchaseRequestItem>();
                    tempCodeNotes[tempCode] = note;
                } else
                {
                    if (!string.IsNullOrWhiteSpace(note))
                    {
                        tempCodeNotes[tempCode] = note;
                    }
                }
                tempCodeGroup[tempCode].Add(
                    new PurchaseRequestItem
                    {
                        ProductVariantId = matchedVariant!.Id,
                        ProductVariantColorId = matchedColor?.Id,
                        SupplierId = matchedSupplier!.Id,
                        Quantity = qty,
                        UnitPrice = matchedQuotation!.QuotePrice,
                        ProductQuotationId = matchedQuotation!.Id
                    });
            }
        }
        var successCount = 0;
        foreach (var group in tempCodeGroup)
        {
            var pr = new PurchaseRequest
            {
                Status = "draft",
                Note = tempCodeNotes[group.Key],
                PurchaseRequestItems = group.Value
            };
            insertRepository.Add(pr);
            successCount++;
        }
        if (successCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var result = new ImportPurchaseRequestsResult
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
            var file1Name = $"ImportErrors_PurchaseRequest_{timestamp}.xlsx";
            var file2Name = $"ImportErrors_PurchaseRequest_WithReason_{timestamp}.xlsx";
            var failedRows = failedRowsData
                .Select(
                    f => new PurchaseRequestImportFailedRow(
                        f.TempCode,
                        f.Note,
                        f.ProductName,
                        f.VariantName,
                        f.ColorName,
                        f.Qty,
                        f.SupplierName,
                        f.Reason))
                .ToList();
            var (file1Bytes, file2Bytes) = excelService.BuildImportErrorReports(failedRows);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file1Name), file1Bytes, cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(errorsDir, file2Name), file2Bytes, cancellationToken)
                .ConfigureAwait(false);
            result.ErrorFileUrl = $"/import-errors/{file1Name}";
            result.ErrorFileWithReasonUrl = $"/import-errors/{file2Name}";
        }
        return Result<ImportPurchaseRequestsResult>.Success(result);
    }
}
