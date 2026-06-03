using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrdersForInvoice
{
    public sealed record GetApprovedPurchaseOrdersForInvoiceQuery : IRequest<Result<PagedResult<PurchaseOrderListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
