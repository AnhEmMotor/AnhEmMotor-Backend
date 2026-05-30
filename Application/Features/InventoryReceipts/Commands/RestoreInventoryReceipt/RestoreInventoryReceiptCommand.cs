using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreInventoryReceipt;

public sealed record RestoreInventoryReceiptCommand : IRequest<Result<InventoryReceiptDetailResponse>>
{
    public int? Id { get; init; }
}
