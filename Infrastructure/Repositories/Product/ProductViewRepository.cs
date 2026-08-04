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
}
