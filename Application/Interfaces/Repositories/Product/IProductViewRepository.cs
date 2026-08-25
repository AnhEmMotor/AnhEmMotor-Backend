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

    public Task<List<DetailedProductView>> GetDetailedRecentViewsAsync(
        int take,
        CancellationToken cancellationToken);

    public Task<(List<ProductViewEntity>, int)> GetProductViewHistoryPagedAsync(
        string? searchKeyword,
        int pageNumber,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);

    public Task<List<ProductViewHistoryDto>> GetProductViewHistoryForChatAsync(
        Guid? customerUserId,
        string? visitorKey,
        int limit,
        CancellationToken cancellationToken);
}

public class ProductViewHistoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int? VariantColorId { get; set; }
    public string? VariantColorName { get; set; }
    public int DwellTimeMs { get; set; }
    public DateTime ViewedAt { get; set; }
}

public sealed record DetailedProductView(
    Guid? CustomerUserId,
    string? VisitorKey,
    int ProductId,
    string ProductName,
    int DwellTimeMs,
    DateTimeOffset ViewedAt);
