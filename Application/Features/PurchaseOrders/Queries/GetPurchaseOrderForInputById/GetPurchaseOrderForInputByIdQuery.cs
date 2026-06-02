using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderForInputById
{
    public sealed record GetPurchaseOrderForInputByIdQuery(int Id) : IRequest<Result<PurchaseOrderDetailForInputResponse?>>;
}
