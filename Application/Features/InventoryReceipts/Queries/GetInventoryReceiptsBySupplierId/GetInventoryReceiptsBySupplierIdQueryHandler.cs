using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId;

public sealed class GetInventoryReceiptsBySupplierIdQueryHandler(IInventoryReceiptReadRepository repository) : IRequestHandler<GetInventoryReceiptsBySupplierIdQuery, Result<PagedResult<InventoryReceiptListResponse>>>
{
    public async Task<Result<PagedResult<InventoryReceiptListResponse>>> Handle(
        GetInventoryReceiptsBySupplierIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InventoryReceiptListResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.InventoryReceiptInfos
                .Any(
                    ii => ii.QuotationProductRow != null &&
                            ii.QuotationProductRow.QuotationReceipt != null &&
                            ii.QuotationProductRow.QuotationReceipt.SupplierId == request.SupplierId),
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
