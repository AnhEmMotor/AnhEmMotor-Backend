using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreManyInventoryReceipts;

public sealed record RestoreManyInventoryReceiptsCommand : IRequest<Result<List<InventoryReceiptDetailResponse>>>
{
    public ICollection<int> Ids { get; init; } = [];
}
