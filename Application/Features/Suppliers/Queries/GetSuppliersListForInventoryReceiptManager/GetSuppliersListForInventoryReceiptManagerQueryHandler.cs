using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSuppliersListForInventoryReceiptManager;

public class GetSuppliersListForInventoryReceiptManagerQueryHandler(ISupplierReadRepository repository) : IRequestHandler<GetSuppliersListForInventoryReceiptManagerQuery, Result<PagedResult<SupplierForInventoryReceiptManagerResponse>>>
{
    public async Task<Result<PagedResult<SupplierForInventoryReceiptManagerResponse>>> Handle(
        GetSuppliersListForInventoryReceiptManagerQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedWithTotalInventoryReceiptAsync<SupplierForInventoryReceiptManagerResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
