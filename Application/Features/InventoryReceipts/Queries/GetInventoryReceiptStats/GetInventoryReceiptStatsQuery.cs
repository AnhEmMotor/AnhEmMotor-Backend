using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStats;

public sealed class GetInventoryReceiptStatsQuery : IRequest<Result<InventoryReceiptStatsResponse>>
{
}
