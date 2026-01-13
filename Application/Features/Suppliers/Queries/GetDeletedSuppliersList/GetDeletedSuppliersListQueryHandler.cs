using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedSuppliersListQuery, Result<PagedResult<SupplierResponse>>>
{
    public async Task<Result<PagedResult<SupplierResponse>>> Handle(
        GetDeletedSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<SupplierEntity, SupplierResponse>(
            query,
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}