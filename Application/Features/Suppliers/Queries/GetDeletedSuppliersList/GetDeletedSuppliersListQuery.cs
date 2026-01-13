using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed record GetDeletedSuppliersListQuery : IRequest<Result<PagedResult<SupplierResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
