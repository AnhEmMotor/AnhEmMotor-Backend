using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ProductViewEntity = Domain.Entities.ProductView;

namespace Infrastructure.Repositories.Product;

public class ProductViewRepository(ApplicationDBContext context) : IProductViewRepository
{
    public void Add(ProductViewEntity view)
    {
        context.ProductViews.Add(view);
    }

    public Task<List<ProductViewSample>> GetRecentViewsAsync(
        Guid? customerUserId,
        string? visitorKey,
        DateTimeOffset since,
        int take,
        CancellationToken cancellationToken)
    {
        if (customerUserId is null && string.IsNullOrWhiteSpace(visitorKey))
        {
            return Task.FromResult(new List<ProductViewSample>());
        }
        var query = context.ProductViews
            .Where(v => v.CreatedAt >= since)
            .Where(v => customerUserId != null ? v.CustomerUserId == customerUserId : v.VisitorKey == visitorKey);
        return query
            .OrderByDescending(v => v.CreatedAt)
            .Take(take)
            .Select(
                v => new ProductViewSample(v.Product!.CategoryId, v.DwellTimeMs, v.CreatedAt ?? DateTimeOffset.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public Task<List<DetailedProductView>> GetDetailedRecentViewsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return context.ProductViews
            .Include(v => v.Product)
            .OrderByDescending(v => v.CreatedAt)
            .Take(take)
            .Select(v => new DetailedProductView(
                v.CustomerUserId, 
                v.VisitorKey, 
                v.ProductId, 
                v.Product != null ? (v.Product.Name ?? "Không rõ") : "Không rõ", 
                v.DwellTimeMs, 
                v.CreatedAt ?? DateTimeOffset.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ProductViewEntity>, int)> GetProductViewHistoryPagedAsync(
        string? searchKeyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.ProductViews
            .Include(pv => pv.CustomerUser)
            .Include(pv => pv.Product)
                .ThenInclude(p => p!.ProductVariants)
                    .ThenInclude(v => v.ProductVariantColors)
            .Include(pv => pv.Variant)
            .Include(pv => pv.VariantColor)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(pv => 
                (pv.VisitorKey != null && pv.VisitorKey.Contains(searchKeyword)) ||
                (pv.CustomerUser != null && pv.CustomerUser.FullName != null && pv.CustomerUser.FullName.Contains(searchKeyword)) ||
                (pv.Product != null && pv.Product.Name != null && pv.Product.Name.Contains(searchKeyword))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(pv => pv.ViewedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<ProductViewHistoryDto>> GetProductViewHistoryForChatAsync(
        Guid? customerUserId,
        string? visitorKey,
        int limit,
        CancellationToken cancellationToken)
    {
        if (customerUserId is null && string.IsNullOrWhiteSpace(visitorKey))
        {
            return Task.FromResult(new List<ProductViewHistoryDto>());
        }

        var query = context.ProductViews
            .Include(pv => pv.Product)
            .Include(pv => pv.Variant)
            .Include(pv => pv.VariantColor)
            .Where(pv => customerUserId != null ? pv.CustomerUserId == customerUserId : pv.VisitorKey == visitorKey)
            .OrderByDescending(pv => pv.ViewedAt)
            .Take(limit)
            .Select(pv => new ProductViewHistoryDto
            {
                ProductId = pv.ProductId,
                ProductName = pv.Product != null ? (pv.Product.Name ?? "Không rõ") : "Không rõ",
                VariantId = pv.VariantId,
                VariantName = pv.Variant != null ? pv.Variant.VariantName : null,
                VariantColorId = pv.VariantColorId,
                VariantColorName = pv.VariantColor != null ? pv.VariantColor.ColorName : null,
                DwellTimeMs = pv.DwellTimeMs,
                ViewedAt = pv.ViewedAt
            });

        return query.ToListAsync(cancellationToken);
    }
}
