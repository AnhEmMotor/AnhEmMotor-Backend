using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var request = command.Request;

        var product = await selectRepository.GetProductWithDetailsByIdAsync(command.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product with Id {command.Id} not found." }]
            });
        }

        if (request.CategoryId.HasValue)
        {
            var category = await selectRepository.GetCategoryByIdAsync(request.CategoryId.Value, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = nameof(request.CategoryId),
                    Message = $"Product category with Id {request.CategoryId} not found."
                });
            }
        }

        if (request.BrandId.HasValue)
        {
            var brand = await selectRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(new ErrorDetail
                {
                    Field = nameof(request.BrandId),
                    Message = $"Brand with Id {request.BrandId} not found."
                });
            }
        }

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
                    errors.Add(new ErrorDetail
                    {
                        Field = "Variants.OptionValueIds",
                        Message = $"Option values not found: {string.Join(", ", missingIds)}."
                    });
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

        foreach (var variant in product.ProductVariants)
        {
            updateRepository.DeleteVariant(variant);
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
                            variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto
                            {
                                ImageUrl = photoUrl.Trim()
                            });
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
                            variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto
                            {
                                ImageUrl = photoUrl.Trim()
                            });
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
}
