using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public record SupplierImportRow(
    string PartnerTypeId,
    string Name,
    string Phone,
    string Email,
    string TaxIdentificationNumber,
    string Address,
    string Notes);

public record SupplierImportFailedRow(
    string PartnerTypeId,
    string Name,
    string Phone,
    string Email,
    string TaxIdentificationNumber,
    string Address,
    string Notes,
    string Reason);

public interface ISupplierExcelService
{
    public byte[] ExportSuppliers(IReadOnlyList<Supplier> suppliers);

    public byte[] BuildImportTemplate();

    public IReadOnlyList<SupplierImportRow>? ParseImportRows(byte[] fileBytes);

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(
        IReadOnlyList<SupplierImportFailedRow> failedRows);
}
