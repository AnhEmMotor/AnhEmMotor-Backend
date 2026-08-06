using Application.ApiContracts.InventoryReport.Responses;

namespace Application.Interfaces.Services.Excel;

public interface IInventoryReportExcelService
{
    public byte[] ExportInventoryReport(IReadOnlyList<InventoryReportSummaryRowResponse> items, int month, int year);
}
