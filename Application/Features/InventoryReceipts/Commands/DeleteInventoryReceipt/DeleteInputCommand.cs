
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteInventoryReceipt;

public sealed record DeleteInventoryReceiptCommand : IRequest<Result>
{
    public int? Id { get; init; }
}
