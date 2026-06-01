using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseInvoices.Commands.ApproveRejectPurchaseInvoice
{
    public sealed record ApproveRejectPurchaseInvoiceCommand(int Id, bool IsApproved, string? Note) : IRequest<Result<PurchaseInvoiceDetailResponse?>>;
}
