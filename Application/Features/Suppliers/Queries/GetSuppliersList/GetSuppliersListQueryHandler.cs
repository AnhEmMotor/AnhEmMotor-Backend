using Application.ApiContracts.Brand.Responses;
using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed class GetSuppliersListQueryHandler(ISupplierReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetSuppliersListQuery, Result<PagedResult<SupplierResponse>>>
{
    public async Task<Result<PagedResult<SupplierResponse>>> Handle(
        GetSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryableWithTotalInput();

        var result = await paginator.ApplyAsync<SupplierWithTotalInputResponse, SupplierResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return result;
    }
}