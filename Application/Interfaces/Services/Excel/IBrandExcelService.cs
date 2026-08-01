using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public record BrandImportRow(string LogoUrl, string Name, string Origin, string Description);

public record BrandImportFailedRow(string LogoUrl, string Name, string Origin, string Description, string Reason);

public interface IBrandExcelService
{
    public byte[] ExportBrands(IReadOnlyList<Brand> brands);

    public byte[] BuildImportTemplate();

    public IReadOnlyList<BrandImportRow> ParseImportRows(byte[] fileBytes);

    public (byte[] WithoutReason, byte[] WithReason) BuildImportErrorReports(IReadOnlyList<BrandImportFailedRow> failedRows);
}
