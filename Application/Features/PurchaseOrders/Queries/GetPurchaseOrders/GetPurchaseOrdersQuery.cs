using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrders
{
    public sealed record GetPurchaseOrdersQuery : IRequest<Result<PagedResult<PurchaseOrderListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
