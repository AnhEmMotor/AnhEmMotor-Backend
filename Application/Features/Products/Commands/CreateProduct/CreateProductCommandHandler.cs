using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;

using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;

using Domain.Entities;
using Mapster;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductCategoryReadRepository productCategoryReadRepository,
    IBrandReadRepository brandReadRepository,
    IProductVariantReadRepository productVariantReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IOptionReadRepository optionReadRepository,
    IProductInsertRepository productInsertRepository,
    IProductVariantInsertRepository productVariantInsertRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        var category = await productCategoryReadRepository.GetByIdAsync(request.CategoryId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(category == null)
        {
            errors.Add(
                Error.NotFound($"Product category with Id {request.CategoryId} not found.", nameof(request.CategoryId)));
        }

        if(request.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(request.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(brand == null)
            {
                errors.Add(Error.BadRequest($"Brand with Id {request.BrandId} not found.", nameof(request.BrandId)));
            }
        }

        if(request.Variants?.Count > 0)
        {
            var slugs = request.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach(var slug in slugs)
            {
                var existing = await productVariantReadRepository.GetBySlugAsync(slug!, cancellationToken)
                    .ConfigureAwait(false);
                if(existing != null)
                {
                    errors.Add(Error.BadRequest($"Slug '{slug}' is already in use.", "Variants.UrlSlug"));
                }
            }
        }

        if(errors.Count > 0)
        {
            return Result<ProductDetailForManagerResponse?>.Failure(errors);
        }

        var optionIdToValueMap = new Dictionary<int, Dictionary<string, int>>();
        var optionNameMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if(request.Variants?.Count > 0)
        {
            var allOptionValues = new Dictionary<int, HashSet<string>>();

            var potentialOptionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(var variantReq in request.Variants)
            {
                if(variantReq.OptionValues?.Count > 0)
                {
                    foreach(var kvp in variantReq.OptionValues)
                    {
                        if(!int.TryParse(kvp.Key, out _))
                        {
                            var keyName = kvp.Key?.Trim();
                            if(!string.IsNullOrWhiteSpace(keyName))
                            {
                                potentialOptionNames.Add(keyName);
                            }
                        }
                    }
                }
            }

            if(potentialOptionNames.Count > 0)
            {
                var existingOptions = await optionReadRepository.GetByNamesAsync(
                    potentialOptionNames,
                    cancellationToken,
                    DataFetchMode.All)
                    .ConfigureAwait(false);

                foreach(var opt in existingOptions)
                {
                    if(opt.Name != null)
                        optionNameMap[opt.Name] = opt.Id;
                }

                var missingNames = potentialOptionNames.Where(n => !optionNameMap.ContainsKey(n)).ToList();
                if(missingNames.Count > 0)
                {
                    var newOptions = missingNames.Select(n => new OptionEntity { Name = n }).ToList();
                    productVariantInsertRepository.AddOptionRange(newOptions);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    foreach(var opt in newOptions)
                    {
                        if(opt.Name != null)
                            optionNameMap[opt.Name] = opt.Id;
                    }
                }
            }

            foreach(var variantReq in request.Variants)
            {
                if(variantReq.OptionValues?.Count > 0)
                {
                    foreach(var kvp in variantReq.OptionValues)
                    {
                        int optionId = 0;
                        if(int.TryParse(kvp.Key, out var parsedId))
                        {
                            optionId = parsedId;
                        } else
                        {
                            var keyName = kvp.Key?.Trim();
                            if(!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                            {
                                optionId = mappedId;
                            }
                        }

                        if(optionId > 0)
                        {
                            var valueName = kvp.Value?.Trim();
                            if(!string.IsNullOrWhiteSpace(valueName))
                            {
                                if(!allOptionValues.TryGetValue(optionId, out HashSet<string>? value))
                                {
                                    value = [];
                                    allOptionValues[optionId] = value;
                                }
                                value.Add(valueName);
                            }
                        }
                    }
                }
            }

            foreach(var optionKvp in allOptionValues)
            {
                var optionId = optionKvp.Key;
                var valueNames = optionKvp.Value;

                var option = await optionReadRepository.GetByIdAsync(optionId, cancellationToken).ConfigureAwait(false);
                if(option == null)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.NotFound($"Option with Id {optionId} not found.", "Variants.OptionValues"));
                }

                var valueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach(var valueName in valueNames)
                {
                    var existingValue = await optionValueReadRepository.GetByIdAndNameAsync(
                        optionId,
                        valueName,
                        cancellationToken)
                        .ConfigureAwait(false);

                    if(existingValue != null)
                    {
                        valueMap[valueName] = existingValue.Id;
                    } else
                    {
                        var newValue = new OptionValueEntity { OptionId = optionId, Name = valueName };
                        optionValueInsertRepository.Add(newValue);
                        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        valueMap[valueName] = newValue.Id;
                    }
                }
                optionIdToValueMap[optionId] = valueMap;
            }
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
            StatusId = "for-sale",
            ProductVariants = []
        };

        if(request.Variants?.Count > 0)
        {
            foreach(var variantReq in request.Variants)
            {
                var variant = new ProductVariant
                {
                    UrlSlug = variantReq.UrlSlug?.Trim(),
                    Price = variantReq.Price,
                    CoverImageUrl = variantReq.CoverImageUrl?.Trim(),
                    ProductCollectionPhotos = [],
                    VariantOptionValues = []
                };

                if(variantReq.PhotoCollection?.Count > 0)
                {
                    foreach(var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                    }
                }

                if(variantReq.OptionValues?.Count > 0)
                {
                    foreach(var kvp in variantReq.OptionValues)
                    {
                        int? resolvedOptionId = null;
                        if(int.TryParse(kvp.Key, out var optionId))
                        {
                            resolvedOptionId = optionId;
                        } else
                        {
                            var keyName = kvp.Key?.Trim();
                            if(!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                            {
                                resolvedOptionId = mappedId;
                            }
                        }

                        if(resolvedOptionId.HasValue)
                        {
                            var valueName = kvp.Value?.Trim();
                            if(!string.IsNullOrWhiteSpace(valueName) &&
                                optionIdToValueMap.TryGetValue(
                                    resolvedOptionId.Value,
                                    out Dictionary<string, int>? value) &&
                                value.TryGetValue(valueName, out var valueId))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = valueId });
                            }
                        }
                    }
                }
                product.ProductVariants.Add(variant);
            }
        }

        productInsertRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<ProductDetailForManagerResponse?>.Success(product.Adapt<ProductDetailForManagerResponse>());
    }
}