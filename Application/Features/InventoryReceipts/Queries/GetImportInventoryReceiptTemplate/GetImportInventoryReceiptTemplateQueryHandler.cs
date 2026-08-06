using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services.Excel;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetImportInventoryReceiptTemplate;

public class GetImportInventoryReceiptTemplateQueryHandler(
    IPurchaseRequestReadRepository purchaseRequestReadRepository,
    IInventoryReceiptExcelService excelService) : IRequestHandler<GetImportInventoryReceiptTemplateQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(
        GetImportInventoryReceiptTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var items = await purchaseRequestReadRepository.GetItemsByPurchaseRequestIdsAsync(
            new[] { request.PurchaseRequestId },
            cancellationToken);
        var content = excelService.BuildImportTemplate(items);
        return Result<byte[]>.Success(content);
    }
}
