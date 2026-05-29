using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Quotation;
using MediatR;
using System.Linq;

namespace Application.Features.PurchaseRequests.Queries.GetQuotedPricesForPR
{
    public sealed class GetQuotedPricesForPRQueryHandler(
        IPurchaseRequestReadRepository prReadRepository,
        IQuotationReadRepository quotationReadRepository) : IRequestHandler<GetQuotedPricesForPRQuery, Result<List<PurchaseRequestQuotedPriceResponse>>>
    {
        public async Task<Result<List<PurchaseRequestQuotedPriceResponse>>> Handle(
            GetQuotedPricesForPRQuery request,
            CancellationToken cancellationToken)
        {
            var pr = await prReadRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id");
            }
            var variantIds = pr.PurchaseRequestItems.Select(x => x.ProductVariantId).Distinct().ToList();
            var quotationRows = await quotationReadRepository.GetApprovedQuotationRowsByVariantsAsync(
                variantIds,
                cancellationToken)
                .ConfigureAwait(false);
            var result = new List<PurchaseRequestQuotedPriceResponse>();
            foreach (var item in pr.PurchaseRequestItems)
            {
                var matchedQuotes = quotationRows.Where(
                    q => q.ProductVariantId == item.ProductVariantId &&
                        q.ProductVariantColorId == item.ProductVariantColorId);
                foreach (var quote in matchedQuotes)
                {
                    result.Add(
                        new PurchaseRequestQuotedPriceResponse
                        {
                            ProductVariantId = quote.ProductVariantId ?? 0,
                            ProductVariantColorId = quote.ProductVariantColorId,
                            SupplierId = quote.QuotationReceipt?.SupplierId ?? 0,
                            SupplierName = quote.QuotationReceipt?.Supplier?.Name ?? string.Empty,
                            QuotePrice = quote.QuotePrice ?? 0,
                            Note = quote.Note
                        });
                }
            }
            return Result<List<PurchaseRequestQuotedPriceResponse>>.Success(result);
        }
    }
}
