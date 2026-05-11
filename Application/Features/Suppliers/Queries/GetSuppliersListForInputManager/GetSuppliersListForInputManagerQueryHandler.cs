using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSuppliersListForInputManager;

public sealed class GetSuppliersListForInputManagerQueryHandler(
    ISupplierReadRepository repository) : IRequestHandler<GetSuppliersListForInputManagerQuery, Result<PagedResult<SupplierForInputManagerResponse>>>
{
    public async Task<Result<PagedResult<SupplierForInputManagerResponse>>> Handle(
        GetSuppliersListForInputManagerQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedWithTotalInputAsync<SupplierForInputManagerResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}