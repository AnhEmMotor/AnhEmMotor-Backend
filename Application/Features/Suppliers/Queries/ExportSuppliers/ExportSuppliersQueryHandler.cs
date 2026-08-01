using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Excel;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.ExportSuppliers;

public class ExportSuppliersQueryHandler(ISupplierReadRepository repository, ISupplierExcelService excelService) : IRequestHandler<ExportSuppliersQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await repository.GetFilteredListAsync(
            request.SieveModel ?? new SieveModel(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var content = excelService.ExportSuppliers(suppliers);
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_nha_cung_cap.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }
}
