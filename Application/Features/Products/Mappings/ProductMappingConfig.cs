using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Responses;
using Domain.Constants;
using Domain.Constants.Order;
using Mapster;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Features.Products.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductEntity, ProductDetailForManagerResponse>()
            .MapWith(src => MapProductToDetailForManagerResponse(src));

        config.NewConfig<ProductEntity, ProductDetailResponse>().MapWith(src => MapProductToDetailResponse(src));

        config.NewConfig<ProductEntity, ProductListRow>()
            .Map(dest => dest.CategoryName, src => src.ProductCategory != null ? src.ProductCategory.Name : null)
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
            .Map(dest => dest.TotalStock, src => CalculateTotalStock(src))
            .Map(dest => dest.TotalBooked, src => CalculateTotalBooked(src));

        config.NewConfig<ProductVariantEntity, VariantRow>()
            .Map(
                dest => dest.Photos,
                src => src.ProductCollectionPhotos.Select(p => p.ImageUrl ?? string.Empty).ToList())
            .Map(
                dest => dest.OptionPairs,
                src => src.VariantOptionValues
                    .Select(
                        vov => new OptionPair
                            {
                                OptionName =
                                    vov.OptionValue != null && vov.OptionValue.Option != null
                                                ? vov.OptionValue.Option.Name
                                                : null,
                                OptionValue = vov.OptionValue != null ? vov.OptionValue.Name : null
                            })
                    .ToList())
            .Map(
                dest => dest.Stock,
                src => src.InputInfos
                        .Where(
                            ii => ii.InputReceipt != null &&
                                        Domain.Constants.Input.InputStatus.IsFinished(ii.InputReceipt.StatusId))
                        .Sum(ii => ii.RemainingCount) ??
                    0)

            .Map(
                dest => dest.HasBeenBooked,
                src => src.OutputInfos
                        .Where(oi => oi.OutputOrder != null && OrderStatus.IsBookingStatus(oi.OutputOrder.StatusId))
                        .Sum(oi => (long?)oi.Count) ??
                    0);

        config.NewConfig<VariantRow, ProductVariantDetailForManagerResponse>()
            .Map(
                dest => dest.OptionValues,
                src => src.OptionPairs
                    .Where(
                        pair => !string.IsNullOrWhiteSpace(pair.OptionName) &&
                                !string.IsNullOrWhiteSpace(pair.OptionValue))
                    .GroupBy(pair => pair.OptionName!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().OptionValue!, StringComparer.OrdinalIgnoreCase))
            .Map(dest => dest.PhotoCollection, src => src.Photos)
            .Map(dest => dest.StatusStockId, src => GetStockStatus(src.Stock - src.HasBeenBooked))
            .Map(dest => dest.InventoryStatus, src => InventoryStatus.OutOfStock);

        config.NewConfig<VariantRow, ProductVariantDetailResponse>()
            .Map(
                dest => dest.OptionValues,
                src => src.OptionPairs
                    .Where(
                        pair => !string.IsNullOrWhiteSpace(pair.OptionName) &&
                                !string.IsNullOrWhiteSpace(pair.OptionValue))
                    .GroupBy(pair => pair.OptionName!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().OptionValue!, StringComparer.OrdinalIgnoreCase))
            .Map(dest => dest.PhotoCollection, src => src.Photos);

        config.NewConfig<ProductVariantEntity, ProductVariantLiteResponse>()
            .MapWith(src => BuildVariantLiteResponse(src));

        config.NewConfig<ProductVariantEntity, ProductVariantLiteResponseForInput>()
            .MapWith(src => BuildVariantLiteResponseForInput(src, null));
    }

    private static ProductDetailForManagerResponse MapProductToDetailForManagerResponse(ProductEntity product)
    { return MapProductToDetailForManagerResponseWithAlertLevel(product, 0); }

    public static ProductDetailForManagerResponse MapProductToDetailForManagerResponseWithAlertLevel(
        ProductEntity product,
        long alertLevel)
    {
        var variantRows = product.ProductVariants.Select(variant => variant.Adapt<VariantRow>()).ToList();

        var variantResponses = variantRows
            .Select(
                row =>
                {
                    var available = row.Stock - row.HasBeenBooked;
                    var inventoryStatus = CalculateInventoryStatus(available, alertLevel);

                    var coverImage = string.IsNullOrWhiteSpace(row.CoverImageUrl)
                        ? row.Photos.FirstOrDefault()
                        : row.CoverImageUrl;

                    return new ProductVariantDetailForManagerResponse
                    {
                        Id = row.Id,
                        ProductId = row.ProductId,
                        UrlSlug = row.UrlSlug,
                        Price = row.Price,
                        CoverImageUrl = coverImage,
                        OptionValues =
                            row.OptionPairs
                                    .Where(
                                        pair => !string.IsNullOrWhiteSpace(pair.OptionName) &&
                                                    !string.IsNullOrWhiteSpace(pair.OptionValue))
                                    .GroupBy(pair => pair.OptionName!, StringComparer.OrdinalIgnoreCase)
                                    .ToDictionary(
                                        g => g.Key,
                                        g => g.First().OptionValue!,
                                        StringComparer.OrdinalIgnoreCase),
                        PhotoCollection = row.Photos,
                        Stock = row.Stock,
                        HasBeenBooked = row.HasBeenBooked,
                        StatusStockId = GetStockStatus(available),
                        InventoryStatus = inventoryStatus
                    };
                })
            .OrderBy(v => InventoryStatus.GetSeverity(v.InventoryStatus))
            .ThenBy(v => v.UrlSlug)
            .ToList();

        var totalStock = variantRows.Sum(v => (long)v.Stock);
        var totalBooked = variantRows.Sum(v => (long)v.HasBeenBooked);
        var productAvailable = totalStock - totalBooked;
        var productInventoryStatus = variantResponses.Count == 0
            ? InventoryStatus.OutOfStock
            : variantResponses.MinBy(v => InventoryStatus.GetSeverity(v.InventoryStatus))!.InventoryStatus;

        return new ProductDetailForManagerResponse
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
            ShortDescription = product.ShortDescription,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            StatusId = product.StatusId,
            CoverImageUrl = variantResponses.FirstOrDefault()?.CoverImageUrl,
            Stock = (int)totalStock,
            HasBeenBooked = totalBooked,
            StatusStockId = GetStockStatus(productAvailable),
            InventoryStatus = productInventoryStatus,
            Variants = variantResponses
        };
    }

    private static ProductDetailResponse MapProductToDetailResponse(ProductEntity product)
    {
        var variantRows = product.ProductVariants
            .Where(v => v.DeletedAt == null)
            .Select(variant => variant.Adapt<VariantRow>())
            .ToList();

        var variantResponses = variantRows
            .Select(variant => variant.Adapt<ProductVariantDetailResponse>())
            .ToList();

        return new ProductDetailResponse
        {
            Id = product.Id,
            Name = product.Name,
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
            ShortDescription = product.ShortDescription,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            CoverImageUrl = variantResponses.FirstOrDefault()?.CoverImageUrl,
            Variants = variantResponses
        };
    }

    private static ProductVariantLiteResponse BuildVariantLiteResponse(ProductVariantEntity variant)
    {
        var optionPairs = variant.VariantOptionValues
            .Select(
                vov => new OptionPair
                {
                    OptionName = vov.OptionValue?.Option?.Name,
                    OptionValue = vov.OptionValue?.Name
                })
            .ToList();

        var variantName = BuildVariantName(optionPairs);
        var productName = variant.Product?.Name;
        var displayName = string.IsNullOrWhiteSpace(variantName)
            ? productName ?? string.Empty
            : $"{productName} ({variantName})";

        var stock = variant.InputInfos
                .Where(
                    ii => ii.InputReceipt != null &&
                            Domain.Constants.Input.InputStatus.IsFinished(ii.InputReceipt.StatusId))
                .Sum(ii => ii.RemainingCount) ??
            0;
        var photos = variant.ProductCollectionPhotos
            .Select(p => p.ImageUrl ?? string.Empty)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        var coverImage = string.IsNullOrWhiteSpace(variant.CoverImageUrl)
            ? photos.FirstOrDefault()
            : variant.CoverImageUrl;

        return new ProductVariantLiteResponse
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            ProductName = productName,
            VariantName = variantName,
            DisplayName = displayName,
            Price = variant.Price,
            StatusId = variant.Product?.StatusId ?? string.Empty,
            CoverImageUrl = coverImage,
            Stock = stock,
            Photos = photos
        };
    }

    private static string BuildVariantName(List<OptionPair> optionPairs)
    {
        if(optionPairs.Count == 0)
        {
            return string.Empty;
        }

        var parts = optionPairs
            .Where(op => !string.IsNullOrWhiteSpace(op.OptionValue))
            .Select(op => op.OptionValue!)
            .ToList();

        return string.Join(" - ", parts);
    }

    public static ProductVariantLiteResponseForInput BuildVariantLiteResponseForInput(
        ProductVariantEntity variant,
        Dictionary<string, string>? translations)
    {
        var optionPairs = variant.VariantOptionValues
            .Select(
                vov => new OptionPair
                {
                    OptionName = vov.OptionValue?.Option?.Name,
                    OptionValue = vov.OptionValue?.Name
                })
            .ToList();

        var productName = variant.Product?.Name ?? string.Empty;
        string displayName;

        if(optionPairs.Count == 0 || optionPairs.All(op => string.IsNullOrWhiteSpace(op.OptionValue)))
        {
            displayName = productName;
        } else
        {
            var parts = optionPairs
                .Where(op => !string.IsNullOrWhiteSpace(op.OptionValue))
                .Select(
                    op =>
                    {
                        var translatedKey = op.OptionName != null &&
                                    translations != null &&
                                    translations.TryGetValue(op.OptionName, out var translated)
                            ? translated
                            : op.OptionName ?? string.Empty;
                        return $"{translatedKey}: {op.OptionValue}";
                    })
                .ToList();

            displayName = $"{productName} ({string.Join(", ", parts)})";
        }

        return new ProductVariantLiteResponseForInput
        {
            Id = variant.Id,
            DisplayName = displayName,
            Price = variant.Price,
            CoverImageUrl = variant.CoverImageUrl
        };
    }

    public static string CalculateInventoryStatus(long availableStock, long alertLevel)
    {
        if(availableStock <= 0)
        {
            return InventoryStatus.OutOfStock;
        }

        if(availableStock <= alertLevel)
        {
            return InventoryStatus.LowStock;
        }

        return InventoryStatus.InStock;
    }

    private static string GetStockStatus(long availableStock)
    { return availableStock > 0 ? "in_stock" : "out_of_stock"; }

    private static long CalculateTotalStock(ProductEntity product)
    {
        return product.ProductVariants
            .SelectMany(variant => variant.InputInfos)
            .Where(
                ii => ii.InputReceipt != null && Domain.Constants.Input.InputStatus.IsFinished(ii.InputReceipt.StatusId))
            .Sum(info => info.RemainingCount ?? 0);
    }

    private static long CalculateTotalBooked(ProductEntity product)
    {
        return product.ProductVariants
            .SelectMany(variant => variant.OutputInfos)
            .Where(info => info.OutputOrder != null && OrderStatus.IsBookingStatus(info.OutputOrder.StatusId))
            .Sum(info => (long)(info.Count ?? 0));
    }

    public static string CalculateProductInventoryStatus(ProductEntity product, long alertLevel)
    {
        var statuses = product.ProductVariants
            .Select(
                variant =>
                {
                    var stock = variant.InputInfos
                        .Where(
                            ii => ii.InputReceipt != null &&
                                    Domain.Constants.Input.InputStatus.IsFinished(ii.InputReceipt.StatusId))
                        .Sum(ii => ii.RemainingCount ?? 0);
                    var booked = variant.OutputInfos
                        .Where(oi => oi.OutputOrder != null && OrderStatus.IsBookingStatus(oi.OutputOrder.StatusId))
                        .Sum(oi => (long)(oi.Count ?? 0));

                    return CalculateInventoryStatus(stock - booked, alertLevel);
                })
            .ToList();

        return statuses.Count == 0 ? InventoryStatus.OutOfStock : statuses.MinBy(InventoryStatus.GetSeverity)!;
    }
}