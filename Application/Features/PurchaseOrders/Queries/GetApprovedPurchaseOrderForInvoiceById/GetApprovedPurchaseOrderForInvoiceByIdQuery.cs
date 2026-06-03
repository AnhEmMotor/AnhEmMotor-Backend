using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrderForInvoiceById
{
    public sealed record GetApprovedPurchaseOrderForInvoiceByIdQuery(int Id, int? ExcludeInvoiceId = null) 
        : IRequest<Result<PurchaseOrderDetailForInvoiceResponse?>>;
}
