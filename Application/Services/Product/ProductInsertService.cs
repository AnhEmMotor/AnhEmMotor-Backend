using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Create;
using Application.ApiContracts.Product.Get;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Product;
using Domain.Entities;
using Domain.Helpers;

namespace Application.Services.Product;

public class ProductInsertService(
    IProductSelectRepository selectRepository,
    IProductInsertRepository insertRepository,
    IProductUpdateRepository updateRepository) : IProductInsertService
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();

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

            foreach (var slug in slugs)
            {
                var existing = await selectRepository.GetVariantBySlugAsync(slug!, includeDeleted: true, cancellationToken).ConfigureAwait(false);
                if (existing != null)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.UrlSlug", Message = $"Slug '{slug}' is already in use." });
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
                var foundIds = optionValues.Select(ov => ov.Id).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();
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

        var product = new Domain.Entities.Product
        {
            Name = request.Name?.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description?.Trim(),
            Weight = request.Weight,
            Dimensions = request.Dimensions?.Trim(),
            Wheelbase = request.Wheelbase?.Trim(),
            SeatHeight = request.SeatHeight,
            GroundClearance = request.GroundClearance,
            FuelCapacity = request.FuelCapacity,
            TireSize = request.TireSize?.Trim(),
            FrontSuspension = request.FrontSuspension?.Trim(),
            RearSuspension = request.RearSuspension?.Trim(),
            EngineType = request.EngineType?.Trim(),
            MaxPower = request.MaxPower?.Trim(),
            OilCapacity = request.OilCapacity,
            FuelConsumption = request.FuelConsumption?.Trim(),
            TransmissionType = request.TransmissionType?.Trim(),
            StarterSystem = request.StarterSystem?.Trim(),
            MaxTorque = request.MaxTorque?.Trim(),
            Displacement = request.Displacement,
            BoreStroke = request.BoreStroke?.Trim(),
            CompressionRatio = request.CompressionRatio?.Trim(),
            StatusId = request.StatusId?.Trim() ?? "for-sale",
            ProductVariants = []
        };

        if (request.Variants?.Count > 0)
        {
            foreach (var variantReq in request.Variants)
            {
                var variant = new ProductVariant
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

        await insertRepository.AddProductAsync(product, cancellationToken).ConfigureAwait(false);
        await insertRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await selectRepository.GetProductWithDetailsByIdAsync(product.Id!.Value, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (created == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve created product." }] });
        }

        var inventoryAlertLevel = await selectRepository.GetInventoryAlertLevelAsync(cancellationToken).ConfigureAwait(false);
        var response = BuildProductDetailResponse(created, inventoryAlertLevel);
        return (response, null);
    }

    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> UpsertProductAsync(
        UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();

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
                    if (request.Id.HasValue && existing.ProductId == request.Id.Value && existing.Id == variantReq.Id)
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
                var foundIds = optionValues.Select(ov => ov.Id).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();
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

        Domain.Entities.Product product;
        bool isUpdate = request.Id.HasValue;

        if (isUpdate)
        {
            var existing = await selectRepository.GetProductWithDetailsByIdAsync(request.Id!.Value, includeDeleted: false, cancellationToken).ConfigureAwait(false);
            if (existing == null)
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {request.Id} not found." }] });
            }

            product = existing;
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

            var existingVariantIds = product.ProductVariants.Select(v => v.Id).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();
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
        }
        else
        {
            product = new Domain.Entities.Product
            {
                Name = request.Name?.Trim(),
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Description = request.Description?.Trim(),
                Weight = request.Weight,
                Dimensions = request.Dimensions?.Trim(),
                Wheelbase = request.Wheelbase?.Trim(),
                SeatHeight = request.SeatHeight,
                GroundClearance = request.GroundClearance,
                FuelCapacity = request.FuelCapacity,
                TireSize = request.TireSize?.Trim(),
                FrontSuspension = request.FrontSuspension?.Trim(),
                RearSuspension = request.RearSuspension?.Trim(),
                EngineType = request.EngineType?.Trim(),
                MaxPower = request.MaxPower?.Trim(),
                OilCapacity = request.OilCapacity,
                FuelConsumption = request.FuelConsumption?.Trim(),
                TransmissionType = request.TransmissionType?.Trim(),
                StarterSystem = request.StarterSystem?.Trim(),
                MaxTorque = request.MaxTorque?.Trim(),
                Displacement = request.Displacement,
                BoreStroke = request.BoreStroke?.Trim(),
                CompressionRatio = request.CompressionRatio?.Trim(),
                StatusId = request.StatusId?.Trim() ?? "for-sale",
                ProductVariants = []
            };

            if (request.Variants?.Count > 0)
            {
                foreach (var variantReq in request.Variants)
                {
                    var variant = new ProductVariant
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

            await insertRepository.AddProductAsync(product, cancellationToken).ConfigureAwait(false);
        }

        await insertRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var result = await selectRepository.GetProductWithDetailsByIdAsync(product.Id!.Value, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve product after upsert." }] });
        }

        var inventoryAlertLevel = await selectRepository.GetInventoryAlertLevelAsync(cancellationToken).ConfigureAwait(false);
        var response = BuildProductDetailResponse(result, inventoryAlertLevel);
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

    private static string GetStockStatus(long availableStock, long inventoryAlertLevel)
    {
        if (availableStock <= 0)
        {
            return "out_of_stock";
        }

        return availableStock <= inventoryAlertLevel ? "low_in_stock" : "in_stock";
    }
}

