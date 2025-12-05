using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Shared;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierReadRepository repository, IPaginator paginator) : IRequestHandler<GetDeletedSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public Task<PagedResult<SupplierResponse>> Handle(
        GetDeletedSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return paginator.ApplyAsync<SupplierEntity, SupplierResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}