using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequests
{
    public sealed class GetPurchaseRequestsQueryHandler(IPurchaseRequestReadRepository repository)
        : IRequestHandler<GetPurchaseRequestsQuery, Result<PagedResult<PurchaseRequestListResponse>>>
    {
        public async Task<Result<PagedResult<PurchaseRequestListResponse>>> Handle(
            GetPurchaseRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await repository.GetPagedAsync<PurchaseRequestListResponse>(
                request.SieveModel!,
                DataFetchMode.ActiveOnly,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
