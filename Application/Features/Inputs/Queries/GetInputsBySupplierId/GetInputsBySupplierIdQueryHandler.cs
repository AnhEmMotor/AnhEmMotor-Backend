using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInputsBySupplierId;

public sealed class GetInputsBySupplierIdQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputsBySupplierIdQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetInputsBySupplierIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InputListResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.InputInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId == request.SupplierId),
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
