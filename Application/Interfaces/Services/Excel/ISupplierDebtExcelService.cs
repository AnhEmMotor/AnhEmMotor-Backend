using Application.ApiContracts.DebtPayment.Responses;

namespace Application.Interfaces.Services.Excel;

public interface ISupplierDebtExcelService
{
    public byte[] ExportSupplierDebts(IReadOnlyList<SupplierDebtResponse> items);
}
