using Application.Common.Models;
using Application.Interfaces.Services.Excel;
using MediatR;

namespace Application.Features.Brands.Queries.GetImportTemplate;

public class GetBrandImportTemplateQueryHandler(IBrandExcelService excelService) : IRequestHandler<GetBrandImportTemplateQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        GetBrandImportTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var content = excelService.BuildImportTemplate();
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_nhap_thuong_hieu.xlsx");
        return await Task.FromResult(Result<FileStreamResult>.Success(fileResult));
    }
}
