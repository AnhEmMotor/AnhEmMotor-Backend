using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.StoreChat.Queries.SearchProductsForStaff;

public class SearchProductsForStaffQueryHandler(IProductReadRepository productReadRepository) : IRequestHandler<SearchProductsForStaffQuery, Result<List<StoreChatProductSearchItemDto>>>
{
    public async Task<Result<List<StoreChatProductSearchItemDto>>> Handle(
        SearchProductsForStaffQuery request,
        CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 50);
        var (items, _, _) = await productReadRepository.GetPagedProductsAsync(
            request.Keyword,
            [],
            [],
            [],
            [],
            null,
            null,
            1,
            limit,
            null,
            null,
            cancellationToken)
            .ConfigureAwait(false);
        return items.Select(
            p => new StoreChatProductSearchItemDto
            {
                ProductId = p.Id,
                ProductName = p.Name ?? string.Empty,
                ImageUrl = p.ProductVariants.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.CoverImageUrl))?.CoverImageUrl 
                           ?? p.ProductVariants.SelectMany(v => v.ProductVariantColors).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.CoverImageUrl))?.CoverImageUrl,
                PriceFrom = p.ProductVariants.Count > 0 ? p.ProductVariants.Min(v => v.Price) : null,
                PriceTo = p.ProductVariants.Count > 0 ? p.ProductVariants.Max(v => v.Price) : null
            })
            .ToList();
    }
}
