using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed record GetSuppliersListQuery : IRequest<Result<PagedResult<SupplierResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

