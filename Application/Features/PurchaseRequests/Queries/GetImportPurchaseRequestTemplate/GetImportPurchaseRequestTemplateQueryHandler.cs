using Application.Common.Models;
using Application.Interfaces.Services.Excel;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetImportPurchaseRequestTemplate;

public class GetImportPurchaseRequestTemplateQueryHandler(IPurchaseRequestExcelService excelService) : IRequestHandler<GetImportPurchaseRequestTemplateQuery, Result<byte[]>>
{
    public Task<Result<byte[]>> Handle(
        GetImportPurchaseRequestTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var content = excelService.BuildImportTemplate();
        return Task.FromResult(Result<byte[]>.Success(content));
    }
}
