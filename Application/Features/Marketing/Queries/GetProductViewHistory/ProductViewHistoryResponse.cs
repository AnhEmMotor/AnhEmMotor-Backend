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
            ProductImageUrl = ResolveProductImageUrl(entity),
            VariantId = entity.VariantId,
            VariantName = entity.Variant?.VariantName ?? entity.Product?.ProductVariants?.FirstOrDefault()?.VariantName,
            VariantColorId = entity.VariantColorId,
            VariantColorName = entity.VariantColor?.ColorName ?? entity.Product?.ProductVariants?.FirstOrDefault()?.ProductVariantColors?.FirstOrDefault()?.ColorName,
            DwellTimeMs = entity.DwellTimeMs,
            ViewedAt = entity.ViewedAt
        };
    }

    private static bool IsValidImage(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) && !url.Contains("dummyimage");
    }

    private static string? ResolveProductImageUrl(ProductView entity)
    {
        if (IsValidImage(entity.VariantColor?.CoverImageUrl))
            return entity.VariantColor!.CoverImageUrl;
        if (IsValidImage(entity.Variant?.CoverImageUrl))
            return entity.Variant!.CoverImageUrl;

        var variants = entity.Product?.ProductVariants;
        if (variants == null || variants.Count == 0)
            return null;

        foreach (var variant in variants.OrderByDescending(v => v.Id == entity.VariantId))
        {
            var colorCover = variant.ProductVariantColors
                .Select(c => c.CoverImageUrl)
                .FirstOrDefault(IsValidImage);
            if (!string.IsNullOrWhiteSpace(colorCover))
                return colorCover;
            if (IsValidImage(variant.CoverImageUrl))
                return variant.CoverImageUrl;
            var photo = variant.ProductCollectionPhotos
                .Select(p => p.ImageUrl)
                .FirstOrDefault(IsValidImage);
            if (!string.IsNullOrWhiteSpace(photo))
                return photo;
        }

        return null;
    }
}
