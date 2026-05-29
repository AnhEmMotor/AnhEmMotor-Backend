using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetDeletedInputsList;

public sealed class GetDeletedInputsListQueryHandler(IInputReadRepository repository) : IRequestHandler<GetDeletedInputsListQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetDeletedInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InputListResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
