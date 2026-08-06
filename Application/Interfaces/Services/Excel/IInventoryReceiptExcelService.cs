using Application.ApiContracts.InventoryReceipt.Responses;
using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public record InventoryReceiptImportRow(
    string PrIdStr,
    string ProductName,
    string VariantName,
    string ColorName,
    string RemQtyStr,
    string QtyStr,
    string Vin,
    string Engine,
    string Note);

public record InventoryReceiptImportFailedRow(
    string PrId,
    string ProductName,
    string VariantName,
    string ColorName,
    string RemQtyStr,
    string Qty,
    string Vin,
    string Engine,
    string Note,
    string Reason);

public interface IInventoryReceiptExcelService
{
    public byte[] ExportInventoryReceipts(
        IReadOnlyList<InventoryReceiptListResponse> receipts,
        IReadOnlyList<InventoryReceiptInfo> items);

    public byte[] BuildImportTemplate(IReadOnlyList<PurchaseRequestItem> items);

    /// <summary>
    /// Returns null when the uploaded file has no worksheet at all; returns an empty list when the worksheet has no
    /// data rows.
    /// </summary>
    public IReadOnlyList<InventoryReceiptImportRow>? ParseImportRows(byte[] fileBytes);

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(
        IReadOnlyList<InventoryReceiptImportFailedRow> failedRows);
}
