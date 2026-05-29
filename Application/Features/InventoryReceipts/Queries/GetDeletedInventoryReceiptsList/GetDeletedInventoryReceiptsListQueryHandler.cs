using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetDeletedInventoryReceiptsList;

public sealed class GetDeletedInventoryReceiptsListQueryHandler(IInventoryReceiptReadRepository repository) : IRequestHandler<GetDeletedInventoryReceiptsListQuery, Result<PagedResult<InventoryReceiptListResponse>>>
{
    public async Task<Result<PagedResult<InventoryReceiptListResponse>>> Handle(
        GetDeletedInventoryReceiptsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InventoryReceiptListResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
