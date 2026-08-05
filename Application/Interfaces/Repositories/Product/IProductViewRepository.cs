using ProductViewEntity = Domain.Entities.ProductView;

namespace Application.Interfaces.Repositories.Product;

public sealed record ProductViewSample(int? CategoryId, int DwellTimeMs, DateTimeOffset ViewedAt);

public interface IProductViewRepository
{
    public void Add(ProductViewEntity view);

    public Task<List<ProductViewSample>> GetRecentViewsAsync(
        Guid? customerUserId,
        string? visitorKey,
        DateTimeOffset since,
        int take,
        CancellationToken cancellationToken);
}
