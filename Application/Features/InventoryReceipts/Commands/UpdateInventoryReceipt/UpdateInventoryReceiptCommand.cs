using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;

public sealed record UpdateInventoryReceiptCommand : IRequest<Result<InventoryReceiptDetailResponse?>>
{
    public int Id { get; init; }

    public int? PurchaseOrderId { get; init; }

    public string? Notes { get; init; }

    public ICollection<UpdateInventoryReceiptInfoRequest> Products { get; init; } = [];
}
