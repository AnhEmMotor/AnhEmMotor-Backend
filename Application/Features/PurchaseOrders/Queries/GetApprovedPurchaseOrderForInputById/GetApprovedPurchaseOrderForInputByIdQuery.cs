using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrderForInputById
{
    public sealed record GetApprovedPurchaseOrderForInputByIdQuery(int Id) : IRequest<Result<PurchaseOrderDetailForInputResponse?>>;
}
