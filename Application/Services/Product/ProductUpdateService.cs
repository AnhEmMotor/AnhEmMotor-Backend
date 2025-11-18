using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Get;
using Application.ApiContracts.Product.ManagePrices;
using Application.ApiContracts.Product.ManageStatus;
using Application.ApiContracts.Product.Update;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Product;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Product;

public class ProductUpdateService(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository) : IProductUpdateService
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();

        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {id} not found." }] });
        }

        var category = await selectRepository.GetCategoryByIdAsync(request.CategoryId!.Value, cancellationToken).ConfigureAwait(false);
        if (category == null)
        {
            errors.Add(new ErrorDetail { Field = nameof(request.CategoryId), Message = $"Product category with Id {request.CategoryId} not found." });
        }

        if (request.BrandId.HasValue)
        {
            var brand = await selectRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(new ErrorDetail { Field = nameof(request.BrandId), Message = $"Brand with Id {request.BrandId} not found." });
            }
        }

        if (request.Variants?.Count > 0)
        {
            var slugs = request.Variants.Select(v => v.UrlSlug?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(new ErrorDetail { Field = "Variants", Message = "Duplicate slugs found within the request." });
            }

            foreach (var variantReq in request.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
            {
                var existing = await selectRepository.GetVariantBySlugAsync(variantReq.UrlSlug!.Trim(), includeDeleted: true, cancellationToken).ConfigureAwait(false);
                if (existing != null)
                {
                    if (existing.ProductId == id && existing.Id == variantReq.Id)
                    {
                        continue;
                    }

                    errors.Add(new ErrorDetail { Field = "Variants.UrlSlug", Message = $"Slug '{variantReq.UrlSlug}' is already in use." });
                }
            }
        }

        var optionValueMap = new Dictionary<int, OptionValue>();
        if (request.Variants?.Count > 0)
        {
            var allOptionValueIds = request.Variants
                .Where(v => v.OptionValueIds?.Count > 0)
                .SelectMany(v => v.OptionValueIds!)
                .Distinct()
                .ToList();

            if (allOptionValueIds.Count > 0)
            {
                var optionValues = await selectRepository.GetOptionValuesByIdsAsync(allOptionValueIds, cancellationToken).ConfigureAwait(false);
                var foundIds = optionValues.Select(ov => ov.Id).Where(ovId => ovId.HasValue).Select(ovId => ovId!.Value).ToHashSet();
                var missingIds = allOptionValueIds.Except(foundIds).ToList();
                if (missingIds.Count > 0)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.OptionValueIds", Message = $"Option values not found: {string.Join(", ", missingIds)}." });
                }

                foreach (var ov in optionValues)
                {
                    if (ov.Id.HasValue)
                    {
                        optionValueMap[ov.Id.Value] = ov;
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        product.Name = request.Name?.Trim();
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Description = request.Description?.Trim();
        product.Weight = request.Weight;
        product.Dimensions = request.Dimensions?.Trim();
        product.Wheelbase = request.Wheelbase?.Trim();
        product.SeatHeight = request.SeatHeight;
        product.GroundClearance = request.GroundClearance;
        product.FuelCapacity = request.FuelCapacity;
        product.TireSize = request.TireSize?.Trim();
        product.FrontSuspension = request.FrontSuspension?.Trim();
        product.RearSuspension = request.RearSuspension?.Trim();
        product.EngineType = request.EngineType?.Trim();
        product.MaxPower = request.MaxPower?.Trim();
        product.OilCapacity = request.OilCapacity;
        product.FuelConsumption = request.FuelConsumption?.Trim();
        product.TransmissionType = request.TransmissionType?.Trim();
        product.StarterSystem = request.StarterSystem?.Trim();
        product.MaxTorque = request.MaxTorque?.Trim();
        product.Displacement = request.Displacement;
        product.BoreStroke = request.BoreStroke?.Trim();
        product.CompressionRatio = request.CompressionRatio?.Trim();
        product.StatusId = request.StatusId?.Trim() ?? "for-sale";

        var existingVariantIds = product.ProductVariants.Select(v => v.Id).Where(vid => vid.HasValue).Select(vid => vid!.Value).ToHashSet();
        var requestVariantIds = request.Variants?.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet() ?? [];
        var toRemove = existingVariantIds.Except(requestVariantIds).ToList();

        foreach (var variantId in toRemove)
        {
            await updateRepository.RemoveVariantAsync(variantId, cancellationToken).ConfigureAwait(false);
        }

        if (request.Variants?.Count > 0)
        {
            foreach (var variantReq in request.Variants)
            {
                ProductVariant? variant;
                if (variantReq.Id.HasValue)
                {
                    variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantReq.Id.Value);
                    if (variant == null)
                    {
                        continue;
                    }

                    variant.UrlSlug = variantReq.UrlSlug?.Trim();
                    variant.Price = variantReq.Price;
                    variant.CoverImageUrl = variantReq.CoverImageUrl?.Trim();

                    variant.ProductCollectionPhotos.Clear();
                    if (variantReq.PhotoCollection?.Count > 0)
                    {
                        foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                        {
                            variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                        }
                    }

                    variant.VariantOptionValues.Clear();
                    if (variantReq.OptionValueIds?.Count > 0)
                    {
                        foreach (var ovId in variantReq.OptionValueIds)
                        {
                            if (optionValueMap.TryGetValue(ovId, out var optionValue))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue
                                {
                                    OptionValueId = ovId,
                                    OptionValue = optionValue
                                });
                            }
                        }
                    }
                }
                else
                {
                    variant = new ProductVariant
                    {
                        UrlSlug = variantReq.UrlSlug?.Trim(),
                        Price = variantReq.Price,
                        CoverImageUrl = variantReq.CoverImageUrl?.Trim(),
                        ProductCollectionPhotos = [],
                        VariantOptionValues = []
                    };

                    if (variantReq.PhotoCollection?.Count > 0)
                    {
                        foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                        {
                            variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                        }
                    }

                    if (variantReq.OptionValueIds?.Count > 0)
                    {
                        foreach (var ovId in variantReq.OptionValueIds)
                        {
                            if (optionValueMap.TryGetValue(ovId, out var optionValue))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue
                                {
                                    OptionValueId = ovId,
                                    OptionValue = optionValue
                                });
                            }
                        }
                    }

                    product.ProductVariants.Add(variant);
                }
            }
        }

        await updateRepository.UpdateProductAsync(product, cancellationToken).ConfigureAwait(false);
        await updateRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (updated == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve updated product." }] });
        }

        var inventoryAlertLevel = await selectRepository.GetInventoryAlertLevelAsync(cancellationToken).ConfigureAwait(false);
        var response = BuildProductDetailResponse(updated, inventoryAlertLevel);
        return (response, null);
    }

    private static readonly string[] BookingStatuses =
    [
        "confirmed_cod",
        "paid_processing",
        "waiting_deposit",
        "deposit_paid",
        "delivering",
        "waiting_pickup"
    ];

    private static ProductDetailResponse BuildProductDetailResponse(Domain.Entities.Product product, long inventoryAlertLevel)
    {
        var variantResponses = product.ProductVariants.Select(variant =>
        {
            var stock = variant.InputInfos.Sum(ii => ii.RemainingCount) ?? 0;
            var booked = variant.OutputInfos.Where(oi => oi.OutputOrder != null && BookingStatuses.Contains(oi.OutputOrder.StatusId ?? string.Empty)).Sum(oi => (long?)oi.Count) ?? 0;
            var availableStock = stock - booked;

            return new ProductVariantDetailResponse
            {
                Id = variant.Id,
                ProductId = product.Id,
                UrlSlug = variant.UrlSlug,
                Price = variant.Price,
                CoverImageUrl = variant.CoverImageUrl,
                OptionValues = variant.VariantOptionValues
                    .Where(vov => vov.OptionValue?.Option?.Name != null && vov.OptionValue.Name != null)
                    .ToDictionary(vov => vov.OptionValue!.Option!.Name!, vov => vov.OptionValue!.Name!, StringComparer.OrdinalIgnoreCase),
                PhotoCollection = [.. variant.ProductCollectionPhotos.Select(p => p.ImageUrl ?? string.Empty)],
                Stock = stock,
                HasBeenBooked = booked,
                StatusStockId = GetStockStatus(availableStock, inventoryAlertLevel)
            };
        })
        .OrderBy(v => v.Stock - v.HasBeenBooked)
        .ThenBy(v => v.UrlSlug)
        .ToList();

        var options = variantResponses
            .SelectMany(v => v.OptionValues)
            .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ProductOptionDetailResponse
            {
                Name = group.Key,
                Values = [.. group.Select(item => item.Value).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v)]
            })
            .OrderBy(opt => opt.Name)
            .ToList();

        var totalStock = variantResponses.Sum(v => v.Stock);
        var totalBooked = variantResponses.Sum(v => v.HasBeenBooked);
        var totalAvailable = totalStock - totalBooked;

        return new ProductDetailResponse
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
            StatusStockId = GetStockStatus(totalAvailable, inventoryAlertLevel),
            Options = options,
            Variants = variantResponses
        };
    }

    public async Task<ErrorResponse?> UpdateProductPriceAsync(int id, UpdateProductPriceRequest request, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Sản phẩm với Id {id} không tồn tại." }] };
        }

        foreach (var variant in product.ProductVariants)
        {
            variant.Price = request.Price;
        }

        await updateRepository.UpdateProductAsync(product, cancellationToken).ConfigureAwait(false);
        await updateRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> UpdateManyProductPricesAsync(UpdateManyProductPricesRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var productNames = request.ProductPrices!.Keys.ToList();
        var allProducts = await selectRepository.GetActiveProducts()
            .Where(p => p.Name != null && productNames.Contains(p.Name))
            .Include(p => p.ProductVariants)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var kvp in request.ProductPrices)
        {
            var productName = kvp.Key;
            var newPrice = kvp.Value;

            var product = allProducts.FirstOrDefault(p => string.Compare(p.Name, productName) == 0);
            if (product == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = productName,
                    Message = $"Sản phẩm '{productName}' không tồn tại."
                });
                continue;
            }

            foreach (var variant in product.ProductVariants)
            {
                variant.Price = newPrice;
            }
        }

        if (errors.Count > 0)
        {
            return new ErrorResponse { Errors = errors };
        }

        foreach (var product in allProducts)
        {
            await updateRepository.UpdateProductAsync(product, cancellationToken).ConfigureAwait(false);
        }

        await updateRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> UpdateProductStatusAsync(int id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Sản phẩm với Id {id} không tồn tại." }] };
        }

        product.StatusId = request.StatusId;

        await updateRepository.UpdateProductAsync(product, cancellationToken).ConfigureAwait(false);
        await updateRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> UpdateManyProductStatusesAsync(UpdateManyProductStatusesRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var products = await selectRepository.GetActiveProducts()
            .Where(p => request.Ids!.Contains(p.Id!.Value))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var id in request.Ids!)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = $"Id: {id}",
                    Message = $"Sản phẩm với Id {id} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return new ErrorResponse { Errors = errors };
        }

        foreach (var product in products)
        {
            product.StatusId = request.StatusId;
            await updateRepository.UpdateProductAsync(product, cancellationToken).ConfigureAwait(false);
        }

        await updateRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    private static string GetStockStatus(long availableStock, long inventoryAlertLevel)
    {
        if (availableStock <= 0)
        {
            return "out_of_stock";
        }

        return availableStock <= inventoryAlertLevel ? "low_in_stock" : "in_stock";
    }
}
