using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptNotes;

public sealed record UpdateInventoryReceiptNotesCommand : IRequest<Result<InventoryReceiptDetailResponse>>
{
    public int Id { get; init; }

    public string? Notes { get; init; }
}
