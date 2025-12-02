using Application.ApiContracts.Product.Common;
using Domain.Constants;
using Mapster;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Features.Products.Mappings;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ApiContracts.Product.Requests.CreateProductRequest, Commands.CreateProduct.CreateProductCommand>(
            )
            .Map(dest => dest.Variants, src => src.Variants ?? new List<ProductVariantWriteRequest>());

        config.NewConfig<ApiContracts.Product.Requests.UpdateProductRequest, Commands.UpdateProduct.UpdateProductCommand>(
            )
            .MapWith(src => new Commands.UpdateProduct.UpdateProductCommand(0, src));

        config.NewConfig<ProductEntity, ApiContracts.Product.Responses.ProductDetailResponse>()
            .MapWith(src => MapProductToDetailResponse(src));

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
            .Map(dest => dest.Stock, src => src.InputInfos.Sum(ii => ii.RemainingCount) ?? 0)
            .Map(
                dest => dest.HasBeenBooked,
                src => src.OutputInfos
                        .Where(oi => oi.OutputOrder != null && OrderStatus.IsBookingStatus(oi.OutputOrder.StatusId))
                        .Sum(oi => (long?)oi.Count) ??
                    0);

        config.NewConfig<VariantRow, ApiContracts.Product.Responses.ProductVariantDetailResponse>()
            .Map(
                dest => dest.OptionValues,
                src => src.OptionPairs
                    .Where(
                        pair => !string.IsNullOrWhiteSpace(pair.OptionName) &&
                                !string.IsNullOrWhiteSpace(pair.OptionValue))
                    .GroupBy(pair => pair.OptionName!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().OptionValue!, StringComparer.OrdinalIgnoreCase))
            .Map(dest => dest.PhotoCollection, src => src.Photos)
            .Map(dest => dest.StatusStockId, src => GetStockStatus(src.Stock - src.HasBeenBooked));

        config.NewConfig<ProductVariantEntity, ProductVariantLiteResponse>()
            .MapWith(src => BuildVariantLiteResponse(src));
    }

    private static ApiContracts.Product.Responses.ProductDetailResponse MapProductToDetailResponse(
        ProductEntity product)
    {
        var variantRows = product.ProductVariants.Select(variant => variant.Adapt<VariantRow>()).ToList();

        var variantResponses = variantRows
            .Select(variant => variant.Adapt<ApiContracts.Product.Responses.ProductVariantDetailResponse>())
            .OrderBy(v => v.Stock - v.HasBeenBooked)
            .ThenBy(v => v.UrlSlug)
            .ToList();

        var totalStock = variantRows.Sum(v => v.Stock);
        var totalBooked = variantRows.Sum(v => v.HasBeenBooked);
        var availableStock = totalStock - totalBooked;

        return new ApiContracts.Product.Responses.ProductDetailResponse
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
            CoverImageUrl = variantResponses.FirstOrDefault()?.CoverImageUrl,
            Stock = totalStock,
            HasBeenBooked = totalBooked,
            StatusStockId = GetStockStatus(availableStock),
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

        var stock = variant.InputInfos.Sum(ii => ii.RemainingCount) ?? 0;
        var photos = variant.ProductCollectionPhotos
            .Select(p => p.ImageUrl ?? string.Empty)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        return new ProductVariantLiteResponse
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            ProductName = productName,
            VariantName = variantName,
            DisplayName = displayName,
            Price = variant.Price,
            StatusId = variant.Product?.StatusId ?? string.Empty,
            CoverImageUrl = variant.CoverImageUrl,
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

    private static string GetStockStatus(long availableStock)
    { return availableStock > 0 ? "in_stock" : "out_of_stock"; }

    private static long CalculateTotalStock(ProductEntity product)
    { return product.ProductVariants.SelectMany(variant => variant.InputInfos).Sum(info => info.RemainingCount ?? 0); }

    private static long CalculateTotalBooked(ProductEntity product)
    {
        return product.ProductVariants
            .SelectMany(variant => variant.OutputInfos)
            .Where(info => info.OutputOrder != null && OrderStatus.IsBookingStatus(info.OutputOrder.StatusId))
            .Sum(info => (long)(info.Count ?? 0));
    }
}