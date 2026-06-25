using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsList;

public class GetInventoryReceiptsListQueryHandler(IInventoryReceiptReadRepository repository) : IRequestHandler<GetInventoryReceiptsListQuery, Result<PagedResult<InventoryReceiptListResponse>>>
{
    public async Task<Result<PagedResult<InventoryReceiptListResponse>>> Handle(
        GetInventoryReceiptsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InventoryReceiptListResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
