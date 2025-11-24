using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Sieve;
using Domain.Enums;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed class GetSuppliersListQueryHandler(ISupplierReadRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public async Task<PagedResult<SupplierResponse>> Handle(GetSuppliersListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        SieveHelper.ApplyDefaultSorting(request.SieveModel);
        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var suppliers = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResult<SupplierResponse>(suppliers.Adapt<List<SupplierResponse>>(), totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
