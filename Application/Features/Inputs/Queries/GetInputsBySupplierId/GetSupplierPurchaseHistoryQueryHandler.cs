using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Primitives;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed class GetSupplierPurchaseHistoryQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetSupplierPurchaseHistoryQuery, Result<PagedResult<SupplierPurchaseHistoryResponse>>>
{
    public async Task<Result<PagedResult<SupplierPurchaseHistoryResponse>>> Handle(
        GetSupplierPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);
        var result = await paginator.ApplyAsync<InputEntity, SupplierPurchaseHistoryResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
