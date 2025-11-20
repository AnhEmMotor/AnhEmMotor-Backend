using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Create;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Product;
using Domain.Entities;
using Domain.Helpers;
using ProductEntity = Domain.Entities.Product;

namespace Application.Services.Product;

public class ProductInsertService(IProductSelectRepository selectRepository, IProductInsertRepository insertRepository, IUnitOfWork unitOfWork) : IProductInsertService
{
    public async Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> CreateProductAsync(
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
                var foundIds = optionValues.Select(ov => ov.Id).ToHashSet();
                var missingIds = allOptionValueIds.Except(foundIds).ToList();
                if (missingIds.Count > 0)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.OptionValueIds", Message = $"Option values not found: {string.Join(", ", missingIds)}." });
                }

                foreach (var ov in optionValues)
                {
                    optionValueMap[ov.Id] = ov;
                }
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        var product = new ProductEntity
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

        await insertRepository.AddAsync(product, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await selectRepository.GetProductWithDetailsByIdAsync(product.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (created == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve created product." }] });
        }

        var response = BuildProductDetailResponse(created);
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

    private static ApiContracts.Product.Select.ProductDetailResponse BuildProductDetailResponse(ProductEntity product)
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
                StatusStockId = GetStockStatus(availableStock)
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

        return new ApiContracts.Product.Select.ProductDetailResponse
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
            StatusStockId = GetStockStatus(totalAvailable),
            Options = options,
            Variants = variantResponses
        };
    }

    private static string GetStockStatus(long availableStock)
    {
        if (availableStock <= 0)
        {
            return "out_of_stock";
        }
        return "in_stock";
    }
}

