using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.SendInventoryReceipt;

public sealed record SendInventoryReceiptCommand : IRequest<Result<InventoryReceiptDetailResponse>>
{
    public int Id { get; init; }
}
