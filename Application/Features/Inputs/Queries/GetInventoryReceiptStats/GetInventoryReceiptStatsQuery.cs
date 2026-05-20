using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInventoryReceiptStats;

public sealed class GetInventoryReceiptStatsQuery : IRequest<Result<InventoryReceiptStatsResponse>>
{
}
