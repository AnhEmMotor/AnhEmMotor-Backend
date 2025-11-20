using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Domain.Constants;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Common;

public static class ProductResponseMapper
{
    public static ProductDetailResponse BuildProductDetailResponse(ProductEntity product)
    {
        var variantRows = product.ProductVariants.Select(variant => new VariantRow
        {
            Id = variant.Id,
            ProductId = product.Id,
            UrlSlug = variant.UrlSlug,
            Price = variant.Price,
            CoverImageUrl = variant.CoverImageUrl,
            Photos = [.. variant.ProductCollectionPhotos.Select(photo => photo.ImageUrl ?? string.Empty)],
            OptionPairs = [.. variant.VariantOptionValues.Select(vov => new OptionPair
            {
                OptionName = vov.OptionValue?.Option?.Name,
                OptionValue = vov.OptionValue?.Name
            })],
            Stock = variant.InputInfos.Sum(ii => ii.RemainingCount) ?? 0,
            HasBeenBooked = variant.OutputInfos
                .Where(oi => oi.OutputOrder != null && OrderBookingStatuses.IsBookingStatus(oi.OutputOrder.StatusId))
                .Sum(oi => (long?)oi.Count) ?? 0
        }).ToList();

        var summary = new ProductListRow
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.ProductCategory?.Name,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            Description = product.Description,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            Wheelbase = product.Wheelbase,
            SeatHeight = product.SeatHeight,
            GroundClearance = product.GroundClearance,
            FuelCapacity = product.FuelCapacity,
            TireSize = product.TireSize,
            FrontSuspension = product.FrontSuspension,
            RearSuspension = product.RearSuspension,
            EngineType = product.EngineType,
            MaxPower = product.MaxPower,
            OilCapacity = product.OilCapacity,
            FuelConsumption = product.FuelConsumption,
            TransmissionType = product.TransmissionType,
            StarterSystem = product.StarterSystem,
            MaxTorque = product.MaxTorque,
            Displacement = product.Displacement,
            BoreStroke = product.BoreStroke,
            CompressionRatio = product.CompressionRatio,
            StatusId = product.StatusId,
            TotalStock = variantRows.Sum(v => v.Stock),
            TotalBooked = variantRows.Sum(v => v.HasBeenBooked)
        };

        return BuildProductDetailResponse(summary, variantRows);
    }

    public static ProductDetailResponse BuildProductDetailResponse(ProductListRow summary, List<VariantRow> variants)
    {
        var variantResponses = variants.Select(variant => new ProductVariantDetailResponse
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            UrlSlug = variant.UrlSlug,
            Price = variant.Price,
            CoverImageUrl = variant.CoverImageUrl,
            OptionValues = variant.OptionPairs
                .Where(pair => !string.IsNullOrWhiteSpace(pair.OptionName) && !string.IsNullOrWhiteSpace(pair.OptionValue))
                .ToDictionary(pair => pair.OptionName!, pair => pair.OptionValue!, StringComparer.OrdinalIgnoreCase),
            PhotoCollection = variant.Photos,
            Stock = variant.Stock,
            HasBeenBooked = variant.HasBeenBooked,
            StatusStockId = GetStockStatus(variant.Stock - variant.HasBeenBooked)
        })
        .OrderBy(v => v.Stock - v.HasBeenBooked)
        .ThenBy(v => v.UrlSlug)
        .ToList();

        var options = variantResponses
            .SelectMany(variant => variant.OptionValues)
            .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ProductOptionDetailResponse
            {
                Name = group.Key,
                Values = [.. group.Select(item => item.Value).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v)]
            })
            .OrderBy(option => option.Name)
            .ToList();

        var availableStock = summary.TotalStock - summary.TotalBooked;

        return new ProductDetailResponse
        {
            Id = summary.Id,
            Name = summary.Name,
            CategoryId = summary.CategoryId,
            CategoryName = summary.CategoryName,
            BrandId = summary.BrandId,
            BrandName = summary.BrandName,
            Description = summary.Description,
            Weight = summary.Weight,
            Dimensions = summary.Dimensions,
            Wheelbase = summary.Wheelbase,
            SeatHeight = summary.SeatHeight,
            GroundClearance = summary.GroundClearance,
            FuelCapacity = summary.FuelCapacity,
            TireSize = summary.TireSize,
            FrontSuspension = summary.FrontSuspension,
            RearSuspension = summary.RearSuspension,
            EngineType = summary.EngineType,
            MaxPower = summary.MaxPower,
            OilCapacity = summary.OilCapacity,
            FuelConsumption = summary.FuelConsumption,
            TransmissionType = summary.TransmissionType,
            StarterSystem = summary.StarterSystem,
            MaxTorque = summary.MaxTorque,
            Displacement = summary.Displacement,
            BoreStroke = summary.BoreStroke,
            CompressionRatio = summary.CompressionRatio,
            StatusId = summary.StatusId,
            CoverImageUrl = variantResponses.FirstOrDefault()?.CoverImageUrl,
            Stock = summary.TotalStock,
            HasBeenBooked = summary.TotalBooked,
            StatusStockId = GetStockStatus(availableStock),
            Options = options,
            Variants = variantResponses
        };
    }

    public static ProductVariantLiteResponse BuildVariantLiteResponse(
        int variantId,
        int? productId,
        string? productName,
        List<OptionPair> optionPairs,
        long? price,
        string? coverImageUrl,
        long stock)
    {
        var variantName = BuildVariantName(optionPairs);
        var displayName = string.IsNullOrWhiteSpace(variantName)
            ? productName ?? string.Empty
            : $"{productName} ({variantName})";

        return new ProductVariantLiteResponse
        {
            Id = variantId,
            ProductId = productId,
            ProductName = productName,
            VariantName = variantName,
            DisplayName = displayName,
            Price = price,
            CoverImageUrl = coverImageUrl,
            Stock = stock
        };
    }

    public static string BuildVariantName(List<OptionPair> optionPairs)
    {
        if (optionPairs.Count == 0)
        {
            return string.Empty;
        }

        var parts = optionPairs
            .Where(op => !string.IsNullOrWhiteSpace(op.OptionValue))
            .Select(op => op.OptionValue!)
            .ToList();

        return string.Join(" - ", parts);
    }

    public static string GetStockStatus(long availableStock)
    {
        return availableStock > 0 ? "in_stock" : "out_of_stock";
    }

    public static List<string> NormalizeStatuses(List<string> statusIds)
    {
        return [.. statusIds
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
