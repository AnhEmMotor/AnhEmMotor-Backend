using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId;

public sealed record GetInventoryReceiptsBySupplierIdQuery : IRequest<Result<PagedResult<InventoryReceiptListResponse>>>
{
    public int SupplierId { get; init; }

    public SieveModel? SieveModel { get; init; }
}
