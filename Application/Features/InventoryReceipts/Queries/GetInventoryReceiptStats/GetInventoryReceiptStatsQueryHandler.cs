using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStats;

public sealed class GetInventoryReceiptStatsQueryHandler(IInventoryReceiptReadRepository repository) : IRequestHandler<GetInventoryReceiptStatsQuery, Result<InventoryReceiptStatsResponse>>
{
    public async Task<Result<InventoryReceiptStatsResponse>> Handle(
        GetInventoryReceiptStatsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetStatsAsync(cancellationToken).ConfigureAwait(false);
        return Result<InventoryReceiptStatsResponse>.Success(result);
    }
}
