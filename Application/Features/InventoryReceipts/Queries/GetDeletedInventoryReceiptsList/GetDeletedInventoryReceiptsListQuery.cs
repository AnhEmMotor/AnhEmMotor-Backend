using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetDeletedInventoryReceiptsList;

public sealed record GetDeletedInventoryReceiptsListQuery : IRequest<Result<PagedResult<InventoryReceiptListResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
