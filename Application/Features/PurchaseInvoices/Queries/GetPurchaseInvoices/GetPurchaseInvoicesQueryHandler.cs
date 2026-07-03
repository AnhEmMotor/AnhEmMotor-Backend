using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices
{
    public class GetPurchaseInvoicesQueryHandler(IPurchaseInvoiceReadRepository repository) : IRequestHandler<GetPurchaseInvoicesQuery, Result<PagedResult<PurchaseInvoiceListResponse>>>
    {
        public async Task<Result<PagedResult<PurchaseInvoiceListResponse>>> Handle(
            GetPurchaseInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            var result = await repository.GetPagedAsync<PurchaseInvoiceListResponse>(
                request.SieveModel!,
                DataFetchMode.ActiveOnly,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
