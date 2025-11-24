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

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierReadRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public async Task<PagedResult<SupplierResponse>> Handle(GetDeletedSuppliersListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);
        SieveHelper.ApplyDefaultSorting(request.SieveModel, DataFetchMode.DeletedOnly);
        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var suppliers = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        var responses = suppliers.Select(s => new SupplierResponse
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            Email = s.Email,
            StatusId = s.StatusId,
            Notes = s.Notes
        }).ToList();

        return new PagedResult<SupplierResponse>(suppliers.Adapt<List<SupplierResponse>>(), totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
