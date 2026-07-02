using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById
{
    public sealed record GetPurchaseInvoiceByIdQuery(int Id)
        : IRequest<Result<PurchaseInvoiceDetailResponse?>>;
}
