using System.Globalization;
using System.Text;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using MediatR;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.ChatTools.Queries.SearchProductsForChat;

public class SearchProductsForChatQueryHandler(IProductReadRepository productReadRepository)
    : IRequestHandler<SearchProductsForChatQuery, Result<ChatToolResult<ChatProductSearchDto>>>
{
    // Chỉ dùng cho fallback khi search LIKE thường (accent-sensitive) không ra kết quả — ví dụ
    // gõ "đĩa" nhưng tên sản phẩm trong DB nhập nhầm không dấu "dĩa". Không sửa GetPagedProductsAsync
    // dùng chung toàn hệ thống (rủi ro/hiệu năng ảnh hưởng trang sản phẩm) — chỉ vá riêng cho tool AI.
    private const int DiacriticFallbackScanSize = 300;

    public async Task<Result<ChatToolResult<ChatProductSearchDto>>> Handle(
        SearchProductsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var (items, totalCount, _) = await productReadRepository.GetPagedProductsAsync(
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

        if (items.Count == 0 && !string.IsNullOrWhiteSpace(request.Keyword))
        {
            (items, totalCount) = await SearchWithoutDiacriticsAsync(request.Keyword, limit, cancellationToken)
                .ConfigureAwait(false);
        }

        var dtos = items.Select(
            p => new ChatProductSearchDto
            {
                ProductId = p.Id,
                ProductName = p.Name ?? string.Empty,
                BrandName = p.Brand?.Name,
                CategoryName = p.ProductCategory?.Name,
                PriceFrom = p.ProductVariants.Count > 0 ? p.ProductVariants.Min(v => v.Price) : null,
                PriceTo = p.ProductVariants.Count > 0 ? p.ProductVariants.Max(v => v.Price) : null,
                VariantCount = p.ProductVariants.Count
            })
            .ToList();
        return new ChatToolResult<ChatProductSearchDto>(dtos, totalCount, totalCount > dtos.Count);
    }

    private async Task<(List<ProductEntity> Items, int TotalCount)> SearchWithoutDiacriticsAsync(
        string keyword, int limit, CancellationToken cancellationToken)
    {
        var (candidates, _, _) = await productReadRepository.GetPagedProductsAsync(
            null, [], [], [], [], null, null, 1, DiacriticFallbackScanSize, null, null, cancellationToken)
            .ConfigureAwait(false);

        var normalizedKeyword = RemoveDiacritics(keyword);
        var matched = candidates.Where(p => ProductMatchesKeyword(p, normalizedKeyword)).ToList();

        return (matched.Take(limit).ToList(), matched.Count);
    }

    private static bool ProductMatchesKeyword(ProductEntity product, string normalizedKeyword)
    {
        return ContainsIgnoreDiacritics(product.Name, normalizedKeyword)
            || ContainsIgnoreDiacritics(product.Brand?.Name, normalizedKeyword)
            || ContainsIgnoreDiacritics(product.ProductCategory?.Name, normalizedKeyword)
            || product.ProductVariants.Any(v => ContainsIgnoreDiacritics(v.VariantName, normalizedKeyword));
    }

    private static bool ContainsIgnoreDiacritics(string? source, string normalizedKeyword)
    {
        return !string.IsNullOrEmpty(source)
            && RemoveDiacritics(source).Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }
}
