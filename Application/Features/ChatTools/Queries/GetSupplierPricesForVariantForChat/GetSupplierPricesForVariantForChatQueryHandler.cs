using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.ProductQuotations;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierPricesForVariantForChat;

public class GetSupplierPricesForVariantForChatQueryHandler(
    IProductQuotationReadRepository quotationReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetSupplierPricesForVariantForChatQuery, Result<ChatToolEnvelope<ChatSupplierPriceListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierPriceListItemDto>>> Handle(
        GetSupplierPricesForVariantForChatQuery request,
        CancellationToken cancellationToken)
    {
        var quotes = await quotationReadRepository.GetByVariantAsync(request.VariantId, cancellationToken)
            .ConfigureAwait(false);
        var sortedQuotes = quotes.OrderBy(q => q.QuotePrice).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = sortedQuotes
            .Take(limit)
            .Select(
                q => new ChatSupplierPriceListItemDto
                {
                    SupplierId = q.SupplierId ?? 0,
                    SupplierName = q.Supplier?.Name ?? string.Empty,
                    VariantId = q.ProductVariantId ?? 0,
                    ColorId = q.ProductVariantColorId,
                    QuotePrice = q.QuotePrice ?? 0,
                    Note = q.Note
                })
            .ToList();
        var inner = new ChatToolResult<ChatSupplierPriceListItemDto>(
            dtos,
            sortedQuotes.Count,
            sortedQuotes.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IProductQuotationReadRepository.GetByVariantAsync",
            new Dictionary<string, string> { ["Sắp xếp"] = "Giá báo giá tăng dần" },
            "gia-nha-cung-cap-theo-bien-the",
            "VND");
        return ChatToolEnvelope<ChatSupplierPriceListItemDto>.Wrap(inner, meta);
    }
}
