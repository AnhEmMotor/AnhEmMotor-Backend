using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetInputsBySupplierId;

public sealed record GetInputsBySupplierIdQuery : IRequest<Result<PagedResult<InputListResponse>>>
{
    public int SupplierId { get; init; }

    public SieveModel? SieveModel { get; init; }
}
