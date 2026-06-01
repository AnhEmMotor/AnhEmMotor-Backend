using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice
{
    public sealed record DeletePurchaseInvoiceCommand(int Id) : IRequest<Result<bool>>;
}
