using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetImportInventoryReceiptTemplate;

public sealed record GetImportInventoryReceiptTemplateQuery : IRequest<Result<byte[]>>
{
    public int PurchaseRequestId { get; init; }
}
