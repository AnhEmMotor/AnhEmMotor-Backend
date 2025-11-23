using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed class GetDeletedSuppliersListQueryHandler(ISupplierSelectRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedSuppliersListQuery, PagedResult<SupplierResponse>>
{
    public async Task<PagedResult<SupplierResponse>> Handle(GetDeletedSuppliersListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetDeletedSuppliers();
        ApplyDefaults(request.SieveModel);
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
            StatusId = s.StatusId
        }).ToList();

        return new PagedResult<SupplierResponse>(responses, totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }

    private static void ApplyDefaults(SieveModel sieveModel)
    {
        sieveModel.Page ??= 1;
        sieveModel.PageSize ??= int.MaxValue;
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = "-id";
        }
        else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
        {
            sieveModel.Sorts = $"{sieveModel.Sorts},-id";
        }
    }
}
