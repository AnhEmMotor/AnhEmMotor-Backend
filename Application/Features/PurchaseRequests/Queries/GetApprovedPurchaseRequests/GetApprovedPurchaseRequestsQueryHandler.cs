using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequests
{
    public sealed class GetApprovedPurchaseRequestsQueryHandler(IPurchaseRequestReadRepository repository) 
        : IRequestHandler<GetApprovedPurchaseRequestsQuery, Result<PagedResult<PurchaseRequestListResponse>>>
    {
        public async Task<Result<PagedResult<PurchaseRequestListResponse>>> Handle(
            GetApprovedPurchaseRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await repository.GetApprovedPagedAsync<PurchaseRequestListResponse>(
                request.SieveModel!,
                DataFetchMode.ActiveOnly,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
