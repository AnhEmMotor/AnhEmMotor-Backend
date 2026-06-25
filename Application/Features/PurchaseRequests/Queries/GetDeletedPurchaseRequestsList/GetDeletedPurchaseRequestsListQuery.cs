using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using Sieve.Models;
using Domain.Primitives;

namespace Application.Features.PurchaseRequests.Queries.GetDeletedPurchaseRequestsList;

public sealed record GetDeletedPurchaseRequestsListQuery : IRequest<Result<PagedResult<PurchaseRequestListResponse>>>
{
    public SieveModel SieveModel { get; init; } = new();
}
