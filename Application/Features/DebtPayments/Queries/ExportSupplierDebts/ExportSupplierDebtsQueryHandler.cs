using Application.Common.Models;
using Application.Features.DebtPayments.Queries.GetSuppliersWithDebt;
using Application.Interfaces.Services.Excel;
using MediatR;
using Sieve.Models;

namespace Application.Features.DebtPayments.Queries.ExportSupplierDebts;

public class ExportSupplierDebtsQueryHandler(IMediator mediator, ISupplierDebtExcelService excelService) : IRequestHandler<ExportSupplierDebtsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportSupplierDebtsQuery request,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersWithDebtQuery { SieveModel = new SieveModel { Page = 1, PageSize = int.MaxValue } };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<FileStreamResult>.Failure(result.Errors);
        }
        var content = excelService.ExportSupplierDebts(result.Value.Items ?? []);
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Cong_no_nha_cung_cap.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }
}
