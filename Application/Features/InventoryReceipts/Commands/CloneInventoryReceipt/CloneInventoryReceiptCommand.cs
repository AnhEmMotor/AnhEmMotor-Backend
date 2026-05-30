using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt;

public sealed record CloneInventoryReceiptCommand : IRequest<Result<InventoryReceiptDetailResponse?>>
{
    public int? Id { get; init; }
}
