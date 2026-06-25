using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequests
{
    public sealed record GetApprovedPurchaseRequestsQuery : IRequest<Result<PagedResult<PurchaseRequestListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
