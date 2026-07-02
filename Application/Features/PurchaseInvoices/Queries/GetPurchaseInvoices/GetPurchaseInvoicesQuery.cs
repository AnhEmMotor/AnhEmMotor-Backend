using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices
{
    public sealed record GetPurchaseInvoicesQuery : IRequest<Result<PagedResult<PurchaseInvoiceListResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
