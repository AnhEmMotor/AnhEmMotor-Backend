using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductPriceListForChat;

public class GetProductPriceListForChatQueryHandler(
    IProductReadRepository productReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetProductPriceListForChatQuery, Result<ChatToolEnvelope<ChatProductPriceListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatProductPriceListItemDto>>> Handle(
        GetProductPriceListForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var filters = string.IsNullOrWhiteSpace(request.Keyword) ? null : $"Name@={request.Keyword}";
        var (items, totalCount) = await productReadRepository
            .GetPagedProductsForPriceManagementAsync(1, limit, filters, null, cancellationToken)
            .ConfigureAwait(false);
        var dtos = items
            .Select(
                p => new ChatProductPriceListItemDto
                {
                    ProductName = p.Name ?? string.Empty,
                    SellPrice = p.ProductVariants.Count > 0 ? p.ProductVariants.Min(v => v.Price ?? 0) : 0
                })
            .ToList();
        var inner = new ChatToolResult<ChatProductPriceListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            filtersApplied["Từ khóa"] = request.Keyword;
        }
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IProductReadRepository.GetPagedProductsForPriceManagementAsync",
            filtersApplied,
            "gia-san-pham",
            "VND");
        return ChatToolEnvelope<ChatProductPriceListItemDto>.Wrap(inner, meta);
    }
}
