using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierReadRepository repository) : IRequestHandler<GetDeletedSuppliersListQuery, Result<PagedResult<SupplierResponse>>>
{
    public async Task<Result<PagedResult<SupplierResponse>>> Handle(
        GetDeletedSuppliersListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<SupplierResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}