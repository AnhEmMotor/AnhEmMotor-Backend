using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed class GetSuppliersListQueryHandler(ISupplierReadRepository repository, IPaginator paginator) : IRequestHandler<GetSuppliersListQuery, Domain.Primitives.PagedResult<SupplierResponse>>
{
    public Task<Domain.Primitives.PagedResult<SupplierResponse>> Handle(
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