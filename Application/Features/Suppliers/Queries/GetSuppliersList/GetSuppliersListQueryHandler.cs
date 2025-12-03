using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Shared;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed class GetSuppliersListQueryHandler(ISupplierReadRepository repository, IPaginator paginator) : IRequestHandler<GetSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public Task<PagedResult<SupplierResponse>> Handle(
        GetSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryableWithTotalInput();

        return paginator.ApplyAsync<SupplierWithTotalInputResponse, SupplierResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}