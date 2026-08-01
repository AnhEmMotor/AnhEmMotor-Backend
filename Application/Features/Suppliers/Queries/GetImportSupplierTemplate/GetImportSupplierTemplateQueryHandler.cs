using Application.Common.Models;
using Application.Interfaces.Services.Excel;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetImportSupplierTemplate;

public class GetImportSupplierTemplateQueryHandler(ISupplierExcelService excelService) : IRequestHandler<GetImportSupplierTemplateQuery, Result<FileStreamResult>>
{
    public Task<Result<FileStreamResult>> Handle(
        GetImportSupplierTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var content = excelService.BuildImportTemplate();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_nhap_doi_tac.xlsx");
        return Task.FromResult(Result<FileStreamResult>.Success(fileResult));
    }
}
