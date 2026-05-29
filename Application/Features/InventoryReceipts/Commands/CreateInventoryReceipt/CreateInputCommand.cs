using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;

public sealed record CreateInventoryReceiptCommand : IRequest<Result<InventoryReceiptDetailResponse?>>
{
    public string? Notes { get; init; }

    public string? StatusId { get; init; }

    public int? PurchaseRequestId { get; init; }

    public List<CreateInventoryReceiptInfoRequest> Products { get; init; } = [];
}
