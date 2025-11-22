using System.Linq;
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

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductInsertRepository insertRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var request = command.Request;

        // Get existing product
        var product = await selectRepository.GetProductWithDetailsByIdAsync(command.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product with Id {command.Id} not found." }]
            });
        }

        // Validate Category (must exist and not soft-deleted)
        if (request.CategoryId.HasValue)
        {
            var category = await selectRepository.GetCategoryByIdAsync(request.CategoryId.Value, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = nameof(request.CategoryId),
                    Message = $"Product category with Id {request.CategoryId} not found or has been deleted."
                });
            }
        }

        // Validate Brand (must exist and not soft-deleted)
        if (request.BrandId.HasValue)
        {
            var brand = await selectRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = nameof(request.BrandId),
                    Message = $"Brand with Id {request.BrandId} not found or has been deleted."
                });
            }
        }

        // Validate unique slugs
        if (request.Variants?.Count > 0)
        {
            var slugs = request.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(new ErrorDetail
                {
                    Field = "Variants",
                    Message = "Duplicate slugs found within the request."
                });
            }

            foreach (var variantReq in request.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
            {
                var existing = await selectRepository.GetVariantBySlugAsync(variantReq.UrlSlug!.Trim(), includeDeleted: true, cancellationToken).ConfigureAwait(false);
                if (existing != null)
                {
                    // Allow same slug if it's the same variant being updated
                    if (existing.ProductId == command.Id && existing.Id == variantReq.Id)
                    {
                        continue;
                    }

                    errors.Add(new ErrorDetail
                    {
                        Field = "Variants.UrlSlug",
                        Message = $"Slug '{variantReq.UrlSlug}' is already in use."
                    });
                }
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        // **UPDATE PRODUCT BASE FIELDS**
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

        // **STEP 1: CLASSIFICATION - Phân loại Variants**
        var currentVariants = await selectRepository.GetVariantsByProductIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        var currentVariantIds = currentVariants.Select(v => v.Id).ToHashSet();

        var inputUpdates = request.Variants?.Where(v => v.Id.HasValue && v.Id.Value > 0).ToList() ?? [];
        var inputInserts = request.Variants?.Where(v => !v.Id.HasValue || v.Id.Value == 0).ToList() ?? [];
        var inputUpdateIds = inputUpdates.Select(v => v.Id!.Value).ToHashSet();

        var idsToDelete = currentVariantIds.Except(inputUpdateIds).ToList();

        // **STEP 2: PROCESSING - Xử lý từng nhóm**

        // DELETE variants not in request
        foreach (var variantId in idsToDelete)
        {
            var variantToDelete = currentVariants.First(v => v.Id == variantId);
            updateRepository.DeleteVariant(variantToDelete);
        }

        // INSERT new variants
        foreach (var variantReq in inputInserts)
        {
            var newVariant = new ProductVariant
            {
                ProductId = command.Id,
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
                    newVariant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                }
            }

            // Process OptionValues
            await ProcessVariantOptionValuesAsync(newVariant, variantReq.OptionValues, cancellationToken).ConfigureAwait(false);

            product.ProductVariants.Add(newVariant);
        }

        // UPDATE existing variants
        foreach (var variantReq in inputUpdates)
        {
            var existingVariant = currentVariants.FirstOrDefault(v => v.Id == variantReq.Id!.Value);
            if (existingVariant == null)
            {
                continue; // Skip if not found (shouldn't happen)
            }

            // Security check: variant must belong to this product
            if (existingVariant.ProductId != command.Id)
            {
                errors.Add(new ErrorDetail
                {
                    Field = "Variants.Id",
                    Message = $"Variant {variantReq.Id} does not belong to product {command.Id}."
                });
                continue;
            }

            // Update variant fields
            existingVariant.UrlSlug = variantReq.UrlSlug?.Trim();
            existingVariant.Price = variantReq.Price;
            existingVariant.CoverImageUrl = variantReq.CoverImageUrl?.Trim();

            // Update photos (replace all)
            existingVariant.ProductCollectionPhotos.Clear();
            if (variantReq.PhotoCollection?.Count > 0)
            {
                foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    existingVariant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                }
            }

            // Process OptionValues (smart update)
            await UpdateVariantOptionValuesAsync(existingVariant, variantReq.OptionValues, cancellationToken).ConfigureAwait(false);
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await selectRepository.GetProductWithDetailsByIdAsync(command.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (updated == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = "Failed to retrieve updated product." }]
            });
        }

        var response = ProductResponseMapper.BuildProductDetailResponse(updated);
        return (response, null);
    }

    /// <summary>
    /// Process OptionValues for NEW variant (INSERT scenario)
    /// </summary>
    private async Task ProcessVariantOptionValuesAsync(
        ProductVariant variant,
        Dictionary<string, string> optionValues,
        CancellationToken cancellationToken)
    {
        if (optionValues == null || optionValues.Count == 0)
        {
            return;
        }

        foreach (var kvp in optionValues)
        {
            if (!int.TryParse(kvp.Key, out var optionId))
            {
                continue;
            }

            var valueName = kvp.Value?.Trim();
            if (string.IsNullOrWhiteSpace(valueName))
            {
                continue;
            }

            // Find or create OptionValue
            var optionValue = await selectRepository.GetOptionValueByNameAsync(optionId, valueName, cancellationToken).ConfigureAwait(false);
            
            if (optionValue == null)
            {
                // Create new OptionValue
                optionValue = new OptionValueEntity
                {
                    OptionId = optionId,
                    Name = valueName
                };
                insertRepository.AddOptionValue(optionValue);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            variant.VariantOptionValues.Add(new VariantOptionValue
            {
                OptionValueId = optionValue.Id
            });
        }
    }

    /// <summary>
    /// Update OptionValues for EXISTING variant (UPDATE scenario)
    /// Implements 3-step algorithm: Prepare, Get Current, Classify & Apply
    /// </summary>
    private async Task UpdateVariantOptionValuesAsync(
        ProductVariant existingVariant,
        Dictionary<string, string> newOptionValues,
        CancellationToken cancellationToken)
    {
        // **STEP 1: Prepare and Process OptionValue (Dictionary Mapping)**
        var targetOptionValueIds = new HashSet<int>();

        if (newOptionValues?.Count > 0)
        {
            foreach (var kvp in newOptionValues)
            {
                if (!int.TryParse(kvp.Key, out var optionId))
                {
                    continue;
                }

                var valueName = kvp.Value?.Trim();
                if (string.IsNullOrWhiteSpace(valueName))
                {
                    continue;
                }

                // Case A: Find existing OptionValue
                var optionValue = await selectRepository.GetOptionValueByNameAsync(optionId, valueName, cancellationToken).ConfigureAwait(false);
                
                if (optionValue != null)
                {
                    targetOptionValueIds.Add(optionValue.Id);
                }
                else
                {
                    // Case B: Create new OptionValue
                    var newOptionValue = new OptionValueEntity
                    {
                        OptionId = optionId,
                        Name = valueName
                    };
                    insertRepository.AddOptionValue(newOptionValue);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    
                    targetOptionValueIds.Add(newOptionValue.Id);
                }
            }
        }

        // **STEP 2: Get Current State of Variant**
        var currentOptionValueIds = existingVariant.VariantOptionValues
            .Where(vov => vov.OptionValueId.HasValue)
            .Select(vov => vov.OptionValueId!.Value)
            .ToHashSet();

        // **STEP 3: Classify Actions (Set Operations)**
        var toInsert = targetOptionValueIds.Except(currentOptionValueIds).ToList(); // New - Current
        var toDelete = currentOptionValueIds.Except(targetOptionValueIds).ToList(); // Current - New
        // var toKeep = targetOptionValueIds.Intersect(currentOptionValueIds).ToList(); // Do nothing

        // Apply DELETE
        foreach (var optionValueId in toDelete)
        {
            var vovToDelete = existingVariant.VariantOptionValues.First(vov => vov.OptionValueId == optionValueId);
            updateRepository.DeleteVariantOptionValue(vovToDelete);
            existingVariant.VariantOptionValues.Remove(vovToDelete);
        }

        // Apply INSERT
        foreach (var optionValueId in toInsert)
        {
            existingVariant.VariantOptionValues.Add(new VariantOptionValue
            {
                VariantId = existingVariant.Id,
                OptionValueId = optionValueId
            });
        }

        // toKeep: Do nothing - already exists
    }
}
