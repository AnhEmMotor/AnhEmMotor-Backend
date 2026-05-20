using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Input;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInventoryReceiptStats;

public sealed class GetInventoryReceiptStatsQueryHandler(IInputReadRepository repository)
    : IRequestHandler<GetInventoryReceiptStatsQuery, Result<InventoryReceiptStatsResponse>>
{
    public async Task<Result<InventoryReceiptStatsResponse>> Handle(
        GetInventoryReceiptStatsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetStatsAsync(cancellationToken).ConfigureAwait(false);
        return Result<InventoryReceiptStatsResponse>.Success(result);
    }
}
