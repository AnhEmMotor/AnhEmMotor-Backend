using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Quotation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Queries.GetApprovedPricesForVariant
{
    public sealed class GetApprovedPricesForVariantQueryHandler(IQuotationReadRepository quotationReadRepository)
        : IRequestHandler<GetApprovedPricesForVariantQuery, Result<List<PurchaseRequestQuotedPriceResponse>>>
    {
        public async Task<Result<List<PurchaseRequestQuotedPriceResponse>>> Handle(
            GetApprovedPricesForVariantQuery request,
            CancellationToken cancellationToken)
        {
            var quotationRows = await quotationReadRepository.GetApprovedQuotationRowsByVariantsAsync(
                [request.VariantId],
                cancellationToken)
                .ConfigureAwait(false);

            var matchedQuotes = quotationRows.Where(
                q => q.ProductVariantId == request.VariantId &&
                    q.ProductVariantColorId == request.ColorId);

            var result = matchedQuotes.Select(quote => new PurchaseRequestQuotedPriceResponse
            {
                ProductVariantId = quote.ProductVariantId ?? 0,
                ProductVariantColorId = quote.ProductVariantColorId,
                SupplierId = quote.QuotationReceipt?.SupplierId ?? 0,
                SupplierName = quote.QuotationReceipt?.Supplier?.Name ?? string.Empty,
                QuotePrice = quote.QuotePrice ?? 0,
                Note = quote.Note,
                QuotationProductRowId = quote.Id
            }).ToList();

            return Result<List<PurchaseRequestQuotedPriceResponse>>.Success(result);
        }
    }
}
