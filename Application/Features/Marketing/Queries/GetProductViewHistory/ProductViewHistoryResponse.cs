using Domain.Entities;

namespace Application.Features.Marketing.Queries.GetProductViewHistory;

public class ProductViewHistoryResponse
{
    public Guid Id { get; set; }
    public Guid? CustomerUserId { get; set; }
    public string? CustomerName { get; set; }
    public string? VisitorKey { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int? VariantColorId { get; set; }
    public string? VariantColorName { get; set; }
    public int DwellTimeMs { get; set; }
    public DateTime ViewedAt { get; set; }
    
    public static ProductViewHistoryResponse FromEntity(ProductView entity)
    {
        return new ProductViewHistoryResponse
        {
            Id = entity.Id,
            CustomerUserId = entity.CustomerUserId,
            CustomerName = entity.CustomerUser?.FullName,
            VisitorKey = entity.VisitorKey,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? string.Empty,
            ProductImageUrl = entity.VariantColor?.CoverImageUrl ?? entity.Variant?.CoverImageUrl,
            VariantId = entity.VariantId,
            VariantName = entity.Variant?.VariantName,
            VariantColorId = entity.VariantColorId,
            VariantColorName = entity.VariantColor?.ColorName,
            DwellTimeMs = entity.DwellTimeMs,
            ViewedAt = entity.ViewedAt
        };
    }
}
