using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrdersForInput
{
    public sealed record GetApprovedPurchaseOrdersForInputQuery : IRequest<Result<PagedResult<PurchaseOrderListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
