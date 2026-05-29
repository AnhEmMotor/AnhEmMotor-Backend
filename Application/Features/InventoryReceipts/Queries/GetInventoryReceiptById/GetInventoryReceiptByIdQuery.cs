using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;

public sealed record GetInventoryReceiptByIdQuery : IRequest<Result<InventoryReceiptDetailResponse?>>
{
    public int Id { get; init; }
}
