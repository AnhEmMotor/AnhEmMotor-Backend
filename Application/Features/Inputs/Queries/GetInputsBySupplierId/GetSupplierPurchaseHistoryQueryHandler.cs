using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Primitives;
using Domain.Constants;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed class GetSupplierPurchaseHistoryQueryHandler(IInputReadRepository repository) : IRequestHandler<GetSupplierPurchaseHistoryQuery, Result<PagedResult<SupplierPurchaseHistoryResponse>>>
{
    public async Task<Result<PagedResult<SupplierPurchaseHistoryResponse>>> Handle(
        GetSupplierPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<SupplierPurchaseHistoryResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.SupplierId == request.SupplierId,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
