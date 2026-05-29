using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInputsBySupplierId;

public sealed class GetSupplierPurchaseHistoryQueryHandler(IInputReadRepository repository) : IRequestHandler<GetSupplierPurchaseHistoryQuery, Result<PagedResult<SupplierPurchaseHistoryResponse>>>
{
    public async Task<Result<PagedResult<SupplierPurchaseHistoryResponse>>> Handle(
        GetSupplierPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<SupplierPurchaseHistoryResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.InputInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId == request.SupplierId),
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
