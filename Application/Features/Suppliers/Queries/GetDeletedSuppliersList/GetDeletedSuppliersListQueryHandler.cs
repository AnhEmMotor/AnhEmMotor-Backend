using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(
    ISupplierReadRepository repository,
    ICustomSievePaginator paginator) : IRequestHandler<GetDeletedSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public async Task<PagedResult<SupplierResponse>> Handle(
        GetDeletedSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<SupplierEntity, SupplierResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}