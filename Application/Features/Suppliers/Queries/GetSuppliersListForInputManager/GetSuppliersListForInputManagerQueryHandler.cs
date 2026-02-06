using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Primitives;
using MediatR;
using System;

namespace Application.Features.Suppliers.Queries.GetSuppliersListForInputManager;

public sealed class GetSuppliersListForInputManagerQueryHandler(
    ISupplierReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetSuppliersListForInputManagerQuery, Result<PagedResult<SupplierForInputManagerResponse>>>
{
    public async Task<Result<PagedResult<SupplierForInputManagerResponse>>> Handle(
        GetSuppliersListForInputManagerQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryableWithTotalInput();

        var result = await paginator.ApplyAsync<SupplierWithTotalInputResponse, SupplierForInputManagerResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}