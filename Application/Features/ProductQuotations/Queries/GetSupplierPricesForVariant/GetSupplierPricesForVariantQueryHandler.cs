using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductQuotations;
using MediatR;
using System.Linq;

namespace Application.Features.ProductQuotations.Queries.GetSupplierPricesForVariant
{
    public class GetSupplierPricesForVariantQueryHandler(IProductQuotationReadRepository quotationReadRepository) : IRequestHandler<GetSupplierPricesForVariantQuery, Result<List<PurchaseRequestQuotedPriceResponse>>>
    {
        public async Task<Result<List<PurchaseRequestQuotedPriceResponse>>> Handle(
            GetSupplierPricesForVariantQuery request,
            CancellationToken cancellationToken)
        {
            var quotationRows = await quotationReadRepository.GetByVariantAsync(request.VariantId, cancellationToken)
                .ConfigureAwait(false);
            var matchedQuotes = quotationRows;
            if (request.ColorId.HasValue)
            {
                matchedQuotes = quotationRows.Where(q => q.ProductVariantColorId == request.ColorId.Value).ToList();
            }
            var result = matchedQuotes.Select(
                quote => new PurchaseRequestQuotedPriceResponse
                {
                    ProductVariantId = quote.ProductVariantId ?? 0,
                    ProductVariantColorId = quote.ProductVariantColorId,
                    SupplierId = quote.SupplierId ?? 0,
                    SupplierName = quote.Supplier?.Name ?? string.Empty,
                    QuotePrice = quote.QuotePrice ?? 0,
                    Note = quote.Note,
                    ProductQuotationId = quote.Id
                })
                .ToList();
            return Result<List<PurchaseRequestQuotedPriceResponse>>.Success(result);
        }
    }
}

