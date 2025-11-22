using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductInsertRepository insertRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();

        // Validate Category (must exist and not soft-deleted)
        var category = await selectRepository.GetCategoryByIdAsync(request.CategoryId!.Value, cancellationToken).ConfigureAwait(false);
        if (category == null)
        {
            errors.Add(new ErrorDetail { Field = nameof(request.CategoryId), Message = $"Product category with Id {request.CategoryId} not found or has been deleted." });
        }

        // Validate Brand (must exist and not soft-deleted)
        if (request.BrandId.HasValue)
        {
            var brand = await selectRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(new ErrorDetail { Field = nameof(request.BrandId), Message = $"Brand with Id {request.BrandId} not found or has been deleted." });
            }
        }

        // Validate unique slugs
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

        // **CRITICAL: Process OptionValues from Variants**
        var optionIdToValueMap = new Dictionary<int, Dictionary<string, int>>(); // optionId -> (valueName -> valueId)

        if (request.Variants?.Count > 0)
        {
            // Collect all unique option IDs and value names from all variants
            var allOptionValues = new Dictionary<int, HashSet<string>>();
            
            foreach (var variantReq in request.Variants)
            {
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        if (int.TryParse(kvp.Key, out var optionId))
                        {
                            var valueName = kvp.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(valueName))
                            {
                                if (!allOptionValues.ContainsKey(optionId))
                                {
                                    allOptionValues[optionId] = [];
                                }
                                allOptionValues[optionId].Add(valueName);
                            }
                        }
                    }
                }
            }

            // Process each option and its values
            foreach (var optionKvp in allOptionValues)
            {
                var optionId = optionKvp.Key;
                var valueNames = optionKvp.Value;

                // Validate Option exists
                var option = await selectRepository.GetOptionByIdAsync(optionId, cancellationToken).ConfigureAwait(false);
                if (option == null)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.OptionValues", Message = $"Option with Id {optionId} not found." });
                    continue;
                }

                var valueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var valueName in valueNames)
                {
                    // Try to find existing OptionValue
                    var existingValue = await selectRepository.GetOptionValueByNameAsync(optionId, valueName, cancellationToken).ConfigureAwait(false);
                    
                    if (existingValue != null)
                    {
                        valueMap[valueName] = existingValue.Id;
                    }
                    else
                    {
                        // Create new OptionValue
                        var newValue = new OptionValueEntity
                        {
                            OptionId = optionId,
                            Name = valueName
                        };
                        insertRepository.AddOptionValue(newValue);
                        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        
                        valueMap[valueName] = newValue.Id;
                    }
                }

                optionIdToValueMap[optionId] = valueMap;
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        // Create Product entity
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

        // Create Variants
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

                // Add photos
                if (variantReq.PhotoCollection?.Count > 0)
                {
                    foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                    }
                }

                // **CRITICAL: Save VariantOptionValues correctly using value NAMES**
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        if (int.TryParse(kvp.Key, out var optionId))
                        {
                            var valueName = kvp.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(valueName) && 
                                optionIdToValueMap.ContainsKey(optionId) &&
                                optionIdToValueMap[optionId].TryGetValue(valueName, out var valueId))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue
                                {
                                    OptionValueId = valueId
                                });
                            }
                        }
                    }
                }

                product.ProductVariants.Add(variant);
            }
        }

        insertRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Retrieve created product with all details
        var created = await selectRepository.GetProductWithDetailsByIdAsync(product.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (created == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve created product." }] });
        }

        var response = ProductResponseMapper.BuildProductDetailResponse(created);
        return (response, null);
    }
}
