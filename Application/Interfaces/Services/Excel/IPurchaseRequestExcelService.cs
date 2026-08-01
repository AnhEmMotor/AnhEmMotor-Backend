using Application.ApiContracts.PurchaseRequest.Responses;
using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public record PurchaseRequestImportRow(
    string TempCode,
    string Note,
    string ProductName,
    string VariantName,
    string ColorName,
    string Qty,
    string SupplierName);

public record PurchaseRequestImportFailedRow(
    string TempCode,
    string Note,
    string ProductName,
    string VariantName,
    string ColorName,
    string Qty,
    string SupplierName,
    string Reason);

public interface IPurchaseRequestExcelService
{
    public byte[] ExportPurchaseRequests(
        IReadOnlyList<PurchaseRequestListResponse> requests,
        IReadOnlyList<PurchaseRequestItem> items,
        IReadOnlyDictionary<int, string> supplierNames);

    public byte[] BuildImportTemplate();

    /// <summary>
    /// Parses the import rows starting at row 5. Returns null if the workbook has no worksheet.
    /// </summary>
    public IReadOnlyList<PurchaseRequestImportRow>? ParseImportRows(byte[] fileBytes);

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(IReadOnlyList<PurchaseRequestImportFailedRow> failedRows);
}
