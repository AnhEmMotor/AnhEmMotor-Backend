using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedSuppliersListQuery, Domain.Primitives.PagedResult<SupplierResponse>>
{
    public Task<Domain.Primitives.PagedResult<SupplierResponse>> Handle(
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