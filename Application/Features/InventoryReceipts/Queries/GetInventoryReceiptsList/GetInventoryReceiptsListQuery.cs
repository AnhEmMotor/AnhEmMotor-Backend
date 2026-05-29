using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsList;

public sealed record GetInventoryReceiptsListQuery : IRequest<Result<PagedResult<InventoryReceiptListResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
