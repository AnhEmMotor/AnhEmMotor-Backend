using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed record GetSupplierPurchaseHistoryQuery : IRequest<Result<PagedResult<SupplierPurchaseHistoryResponse>>>
{
    public int SupplierId { get; init; }

    public SieveModel? SieveModel { get; init; }
}
