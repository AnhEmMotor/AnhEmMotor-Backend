using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Excel;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.ExportPurchaseRequests;

public class ExportPurchaseRequestsQueryHandler(
    IPurchaseRequestReadRepository readRepository,
    ISupplierReadRepository supplierReadRepository,
    IPurchaseRequestExcelService excelService) : IRequestHandler<ExportPurchaseRequestsQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportPurchaseRequestsQuery request, CancellationToken cancellationToken)
    {
        request.SieveModel.PageSize = 100000;
        request.SieveModel.Page = 1;
        var pagedResult = await readRepository.GetPagedAsync<PurchaseRequestListResponse>(
            request.SieveModel,
            DataFetchMode.ActiveOnly,
            cancellationToken)
            .ConfigureAwait(false);
        var requests = pagedResult.Items ?? [];
        var requestIds = requests.Select(r => r.Id).ToList();
        var items = new List<PurchaseRequestItem>();
        if (requestIds.Any())
        {
            items = await readRepository.GetItemsByPurchaseRequestIdsAsync(requestIds, cancellationToken)
                .ConfigureAwait(false);
        }
        var suppliers = await supplierReadRepository.GetAllAsync(cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var supplierDict = suppliers.ToDictionary(s => s.Id, s => s.Name ?? string.Empty);
        return excelService.ExportPurchaseRequests(requests, items, supplierDict);
    }
}
