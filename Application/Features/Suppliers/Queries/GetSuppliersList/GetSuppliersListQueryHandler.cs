using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Shared;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed class GetSuppliersListQueryHandler(ISupplierReadRepository repository, IPaginator paginator) : IRequestHandler<GetSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public Task<PagedResult<SupplierResponse>> Handle(
        GetSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<SupplierEntity, SupplierResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}