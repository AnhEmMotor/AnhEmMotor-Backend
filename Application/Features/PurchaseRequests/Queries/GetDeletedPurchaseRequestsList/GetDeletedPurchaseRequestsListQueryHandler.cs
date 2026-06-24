using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using MediatR;
using Domain.Primitives;

namespace Application.Features.PurchaseRequests.Queries.GetDeletedPurchaseRequestsList;

public class GetDeletedPurchaseRequestsListQueryHandler(
    IPurchaseRequestReadRepository readRepository) : IRequestHandler<GetDeletedPurchaseRequestsListQuery, Result<PagedResult<PurchaseRequestListResponse>>>
{
    public async Task<Result<PagedResult<PurchaseRequestListResponse>>> Handle(GetDeletedPurchaseRequestsListQuery request, CancellationToken cancellationToken)
    {
        var result = await readRepository.GetPagedAsync<PurchaseRequestListResponse>(request.SieveModel, DataFetchMode.DeletedOnly, cancellationToken).ConfigureAwait(false);
        return Result<PagedResult<PurchaseRequestListResponse>>.Success(result);
    }
}
