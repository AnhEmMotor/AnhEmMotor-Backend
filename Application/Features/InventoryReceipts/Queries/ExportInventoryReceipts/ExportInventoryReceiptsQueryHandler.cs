using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Services.Excel;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.ExportInventoryReceipts;

public class ExportInventoryReceiptsQueryHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptExcelService excelService) : IRequestHandler<ExportInventoryReceiptsQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportInventoryReceiptsQuery request, CancellationToken cancellationToken)
    {
        request.SieveModel.PageSize = 100000;
        request.SieveModel.Page = 1;
        var pagedResult = await readRepository.GetPagedAsync<InventoryReceiptListResponse>(
            request.SieveModel,
            DataFetchMode.ActiveOnly,
            null,
            cancellationToken)
            .ConfigureAwait(false);
        var receipts = pagedResult.Items ?? [];
        var receiptIds = receipts.Where(r => r.Id != null).Select(r => r.Id!.Value).ToList();
        var items = new List<InventoryReceiptInfo>();
        if (receiptIds.Any())
        {
            items = await readRepository.GetInfosByInventoryReceiptIdsAsync(receiptIds, cancellationToken)
                .ConfigureAwait(false);
        }
        return excelService.ExportInventoryReceipts(receipts, items);
    }
}
