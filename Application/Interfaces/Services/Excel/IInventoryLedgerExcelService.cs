using Domain.Entities;

namespace Application.Interfaces.Services.Excel;

public interface IInventoryLedgerExcelService
{
    public byte[] ExportInventoryLedger(IReadOnlyList<InventoryLedger> entries, DateTime? startDate, DateTime? endDate);
}
