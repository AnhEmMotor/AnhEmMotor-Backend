using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById
{
    public sealed record GetPurchaseOrderByIdQuery(int Id) : IRequest<Result<PurchaseOrderDetailResponse?>>;
}
