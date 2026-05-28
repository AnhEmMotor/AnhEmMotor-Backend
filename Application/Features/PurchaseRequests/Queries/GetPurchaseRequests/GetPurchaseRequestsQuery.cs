using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequests
{
    public sealed record GetPurchaseRequestsQuery : IRequest<Result<PagedResult<PurchaseRequestListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
