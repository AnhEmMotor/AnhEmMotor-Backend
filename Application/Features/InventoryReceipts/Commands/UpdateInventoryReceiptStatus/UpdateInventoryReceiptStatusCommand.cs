using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;

public sealed record UpdateInventoryReceiptStatusCommand : IRequest<Result<InventoryReceiptDetailResponse>>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;
}

